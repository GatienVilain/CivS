using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitView : MonoBehaviour,IClick
{
    public Unit unit;       //TODO: eventuellement les mettre en priv� et faire des m�thode pour les changer et y acc�der
    public HexMap hexMap;
    public Hex hex;

    private Animator animator; //O� changer l'animation
    [SerializeField] private RuntimeAnimatorController idle;
    [SerializeField] private RuntimeAnimatorController walking;
    [SerializeField] private RuntimeAnimatorController mining;
    [SerializeField] private GameObject sledgeHammer;
    [SerializeField] private GameObject selectedCylinder;
    private bool isMining = false;
    private bool isMoving = false;

    Vector3 newPosition;
    Vector3 currentVelocity;
    float smoothTime = 0.5f;

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        newPosition = this.transform.position;
        animator.runtimeAnimatorController = idle;
        sledgeHammer.SetActive(false);
        selectedCylinder.SetActive(false);
    }

    public void OnUnitMoved(Hex oldHex, Hex nexHex)
    {
        //Animate the unit moving from oldHex to newHex
        this.transform.position = oldHex.PositionFromCamera();
        newPosition = nexHex.PositionFromCamera();
        currentVelocity = Vector3.zero;

        hex = nexHex;

        isMoving = true;
        animator.runtimeAnimatorController = walking;

        if(Vector3.Distance(this.transform.position,newPosition) > 2)
        {
            //This OnUNitMoved is considerably more than the expected move
            //between two adjacent tiles -- it's probably a map seam thing 
            // So just teleport
            this.transform.position = newPosition;

        }
    }

    public void StopMoving()
    {
        isMoving = false;
        animator.runtimeAnimatorController = idle;
        gameObject.transform.parent = hexMap.GetHexeGameobjectFromDictionnary(hex).transform; //Change le parent du gameobject de l'unit�
    }

    public void OnLeftClickAction()
    {
        //*************TODO: s�lectionne le personnage (Change UI, surbrillance, surbrillance des hexagone de sa liste hexpath)
        selectedCylinder.SetActive(true);
        if(unit.hexPath != null && unit.hexPath.Count != 0)
        {
            foreach(Hex hex in unit.hexPath)
            {
                hexMap.GetHexeGameobjectFromDictionnary(hex).GetComponent<HexComponent>().HighlightHexagon();
            }
        }
    }

    public void OnLeftClickOnOtherAction()
    {
        selectedCylinder.SetActive(false);
        if (unit.hexPath != null && unit.hexPath.Count != 0)
        {
            foreach (Hex hex in unit.hexPath)
            {
                hexMap.GetHexeGameobjectFromDictionnary(hex).GetComponent<HexComponent>().UnHighlightHexagon();
            }
        }
    }

    public void OnRightClickAction(GameObject gameobject)
    {
        if(gameobject.tag == "Hexagon")
        {
            HexComponent targetHexComponent = gameobject.GetComponent<HexComponent>();

            //La r�f�rence va �tre soit la case de l'unit�, soit la derni�re case dans sa liste de d�placements
            Hex sourceHex = (unit.hexPath.Count == 0)? hex : unit.hexPath.Last(); 

            if (Hex.Distance(sourceHex, targetHexComponent.hex) <= 1) //Si on clic droit sur une case � 1 case de distance, on l'ajoute � la liste
            {
                if (targetHexComponent.hex.iswalkable)
                {
                    unit.AddToHexPath(targetHexComponent.hex);
                    targetHexComponent.HighlightHexagon();
                }
                else
                {
                    Debug.Log("Destination non joignable");
                }
            }
            else //Sinon on trouve la liste de cases qui va permettre de la rejoindre le plus vite possible � partir de la source ("sourceHex")
            {
                //Renvoie le liste de case permettant de rejoindre la cible le plus vite possible.
                List<HexMap.Node> path = DijkstraPathfinding(hexMap.GetPathFindingGraph(), sourceHex, targetHexComponent.hex);

                if(path == null) //Si on ne peut pas rejoindre la case (parce qu'il y a de l'eau ou des montagnes qui l'enclavent)
                {
                    Debug.Log("Destionation non joignable");
                }
                else //Sinon on l'ajoute au chemin � parcourir et l'affiche.
                { 
                    foreach (HexMap.Node node in path)
                    {
                        unit.AddToHexPath(node.hex);
                        hexMap.GetHexeGameobjectFromDictionnary(node.hex).GetComponent<HexComponent>().HighlightHexagon();
                    }
                }
            }
            //TODO �ventuellement tenir compte des ennemis.
            
        }
        else if(gameobject == gameObject) //Si clic droit sur l'unit� --> reset la liste de d�placements de l'unit�
        {
            foreach (Hex hex in unit.hexPath)
            {
                hexMap.GetHexeGameobjectFromDictionnary(hex).GetComponent<HexComponent>().UnHighlightHexagon();
            }
            unit.SetHexPath(new Hex[0]);
        }
        //*****************TODO: else if(gameobject.tag == "Ennemy") { DoAttack() }
    }

    public List<HexMap.Node> DijkstraPathfinding(HexMap.Node[,] pathFindingGraph, Hex unitHex, Hex targetHex)
    {
        /*Renvoie une liste des noeuds � parcourrir pour arriver � la destination le plus vite possible � partir du
        pathfindingGraph (array de listes chain�es d�crivant les voisins de chaque noeuds), l'hexagone sur lequel est
        l'unit� et l'hexagone cibl�.*/

        HexMap.Node source = pathFindingGraph[unitHex.Q, unitHex.R]; //Noeud de l'hexagone de l'unit�
        HexMap.Node target = pathFindingGraph[targetHex.Q,targetHex.R]; //Noeud de l'hexagone de la case souhait�e

        Dictionary<HexMap.Node, float> distance = new Dictionary<HexMap.Node, float>(); //Distance de chaque noeud par rapport au noeud source

        //Noeud Pr�c�dent pour chaque noeud pour retourner au noeud source le plus vite possible
        Dictionary<HexMap.Node, HexMap.Node> previousNodes = new Dictionary<HexMap.Node, HexMap.Node>(); 

        List<HexMap.Node> unvisitedNodesQueue = new List<HexMap.Node>(); //Liste des noeuds qui n'ont pas �t� examin�s encore

        //Initialise les dictionnaires et remplit la liste des unvisited nodes
        distance[source] = 0;
        previousNodes[source] = null;
        foreach (HexMap.Node v in pathFindingGraph)
        {
            if (v != source)
            {
                distance[v] = Mathf.Infinity;
                previousNodes[v] = null;
            }
            unvisitedNodesQueue.Add(v);
        }


        while (unvisitedNodesQueue.Count > 0)
        {

            //Le noeud u va �tre le unvisited node avec la distance la plus faible (le premier va �tre la source).
            HexMap.Node u = null;

            foreach(HexMap.Node nextU in unvisitedNodesQueue)
            {
                if(u == null || distance[nextU] < distance[u])
                {
                    u = nextU;
                }
            }
            
            if(u == target)
            {
                break; //sort de la boucle While car on a trouv� la target
            }

            unvisitedNodesQueue.Remove(u); //On remove le noeud u consid�r� de la liste des noeuds non �valu�s

            foreach (HexMap.Node v in u.neighbours) 
            {
                //On calcule sa distance relative au noeud source ou le co�t en d�placement pour s'y d�placer
                //float alt = distance[u] + u.Distance(v); 
                float alt = distance[u] + v.hex.BaseMovementCost();
                if (alt < distance[v])
                {
                    distance[v] = alt;
                    previousNodes[v] = u; //On lui attribue son noeud pr�c�dent
                }
            }
        }
        //Soit on a trouv� le chemin le plus court, soit il n'y a pas de chemin
        if (previousNodes[target] == null)
        {
            //pas de route
            return null;
        }
        else
        {
            //On a trouv� le chemin le plus court.
            List<HexMap.Node> path = new List<HexMap.Node>(); //Liste qui va recueillir la liste des noeuds de la target jusqu'� la source
            HexMap.Node current = target;
            while (current != null)
            {
                path.Add(current);
                current = previousNodes[current];
            }
            path.Reverse(); //On inverse la liste pour partir de la source vers la target
            if (path[0] == source)
            {
                //On enl�ve la source si elle est pr�sente car l'unit� �tant d�j� dessus, elle ne doit pas se d�placer sur l'hexagone correspondant
                path.RemoveAt(0); 
            }
            return path;
        }
    }

    void Update()
    {
        if (isMoving)
        {
            this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref currentVelocity, smoothTime);
            if(Vector3.Distance(this.transform.position, newPosition) <= 0.1f)
            {
                StopMoving();
            }
        }
    }
}

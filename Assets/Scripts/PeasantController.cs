using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

public class PeasantController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navAgent; //Build-In IA sur Unity pour les personnages (ne pas oublier de d�finir le MeshAgent dans window->ai->navigation)
    [SerializeField] private Animator animator; //O� changer l'animation
    [SerializeField] private AnimatorController idle;
    [SerializeField] private AnimatorController walking;
    [SerializeField] private AnimatorController mining;
    [SerializeField] private GameObject sledgeHammer;
    private bool isMining = false;
    private bool isWalking = false;
    private Vector3 lastDestination;

    [SerializeField] private Vector3 destination;
    


    private void Start()
    {
        //r�glage de base
        destination = transform.position;
        lastDestination = destination;
        sledgeHammer.SetActive(false);
    }

    private void SetDestination(Vector3 newDestination) //Change la destination en v�rifiant si un objet ne se trouve pas dessus (auquel, trouve un point � c�t�)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newDestination, out hit, 3f, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
    }

    private void GoMining() //Lance et stop l'animation de building
    {
        if (!isMining)
        {
            isMining = true;
            sledgeHammer.SetActive(true);
            animator.runtimeAnimatorController = mining;
        }
        else
        {
            isMining=false;
            animator.runtimeAnimatorController = idle;
            sledgeHammer.SetActive(false);
        }
    }

    void Update()
    {
        if(destination != lastDestination) //Change la destination et lance la marche
        {
            if (isMining)
            {
                GoMining();
            }
            isWalking = true;
            animator.runtimeAnimatorController = walking;
            lastDestination = destination;
            SetDestination(destination);
        }
        if(navAgent.remainingDistance < .1f && isWalking) //arr�te la marche si le personnage est arriv�
        {
            isWalking=false;
            animator.runtimeAnimatorController = idle;
        }
        if (Input.GetKeyDown(KeyCode.M) && !isWalking)
        {
            GoMining();
        }
    }
}

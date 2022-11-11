using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexComponent : MonoBehaviour,IClick
{
    [SerializeField] GameObject selectedHexagon;

    public Hex hex;
    public HexMap hexMap;

    private void Start()
    {
        selectedHexagon.SetActive(false);
    }

    public void UpdatePosition()
    {
        this.transform.position = hex.PositionFromCamera(
            Camera.main.transform.position,
            hexMap.numberRows,
            hexMap.numberColumns
        );
    }

    public void OnLeftClickAction()
    {
        //*****************TODO: s�lectionne l'hexagone (surbrillance, UI, ...)
        HighlightHexagon();
    }

    public void OnLeftClickOnOtherAction()
    {
        UnHighlightHexagon();
    }
    public void HighlightHexagon()
    {
        selectedHexagon.SetActive(true);
    }
    public void UnHighlightHexagon()
    {
        selectedHexagon.SetActive(false);
    }
    public void OnRightClickAction(GameObject gameobject)
    {
        //Do nothing, peut-�tre d�s�lectionner l'hexagone ?
    }
}

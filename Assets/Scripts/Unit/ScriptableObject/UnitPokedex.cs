using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class UnitPokedex : ScriptableObject
{
    //Regroupe les diff�rentes fiches de charact�ristiques de l'unit�.
    public List<UnitSheet> units = new List<UnitSheet>();
}


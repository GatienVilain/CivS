using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class UnitSheet : ScriptableObject
{
    //Charact�ristiques de l'unit�
    public new string name;
    public int strength;
    public int movement;
    public List<int> Cost;
    public GameObject Prefab;
}

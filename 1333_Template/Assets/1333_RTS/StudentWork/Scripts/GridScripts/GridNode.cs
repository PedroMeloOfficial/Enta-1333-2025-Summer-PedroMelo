using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridNode
{
    public GameObject Occupant;
    public bool HasOccupant() => Occupant != null;
    public GameObject GetOccupant() => Occupant;
    public string Name; // Grid Index
    public Vector3 WorldPosition;
    public bool Walkable;
    public int Weight;
    public Color TerrainColor;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GridNode
{
    public string Name; // Grid Index
    public Vector3 WorldPosition;
    public bool Walkable;
    public int Weight;
    public Color TerrainColor;
}
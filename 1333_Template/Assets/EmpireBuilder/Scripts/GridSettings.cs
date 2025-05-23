using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/GameSettings")]
public class GridSettings : ScriptableObject
{
    [SerializeField] private int gridSizeX = 10;
    [SerializeField] private int gridSizeY = 10;
    [SerializeField] private float nodeSize = 1f;
    [SerializeField] private bool useXYPlane = true;

    public int GridSizeX => gridSizeX;
    public int GridSizeY => gridSizeY;
    public float NoseSize => nodeSize;
    public bool UseXYPlane => useXYPlane;
}
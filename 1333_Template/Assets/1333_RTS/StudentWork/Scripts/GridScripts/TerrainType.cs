using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/TerrainType")]
public class TerrainType : ScriptableObject
{
    [SerializeField] private string terrainName = "Default";
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private bool walkable = true;
    [SerializeField] private int movementCost = 1;

    [Header("Spawn Weight (for grid generation probability)")]
    [SerializeField] private int spawnWeight = 1;  // New field

    public string TerrainName => terrainName;
    public Color GizmoColor => gizmoColor;
    public bool Walkable => walkable;
    public int MovementCost => movementCost;
    public int SpawnWeight => spawnWeight;  // New getter
}
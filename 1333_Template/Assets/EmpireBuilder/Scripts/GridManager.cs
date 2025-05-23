using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings;
    public GridSettings GridSettings => gridSettings;

    [Header("Terrain List")]
    [SerializeField] private List<TerrainType> terrainTypes = new();

    private GridNode[,] gridNodes;

    [Header("Debug for editor playmode only")]
    [SerializeField] private List<GridNode> AllNode = new();

    public bool IsInitialized { get; private set; } = false;
    public void InitiazeGrid()
    {
        gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];

        for(int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for(int y = 0; y < gridSettings.GridSizeY; y++)
            {
                Vector3 worldPos = gridSettings.UseXYPlane ? new Vector3(x, 0, y) * gridSettings.NoseSize : new Vector3(x, y, 0) * gridSettings.NoseSize;

                TerrainType newTerrain = terrainTypes[Random.Range(0, terrainTypes.Count)];

                GridNode node = new GridNode
                {
                    Name = $"Cell_{(x + gridSettings.GridSizeX * x + y)}",
                    WorldPosition = worldPos,
                    Walkable = newTerrain.Walkable,
                    Weight = newTerrain.MovementCost,
                    TerrainColor = newTerrain.GizmoColor
                };
                gridNodes[x, y] = node;
            }
        }
        IsInitialized = true;
    }

    public void SetWalkable(int x, int y, bool walkable)
    {
        gridNodes[x, y].Walkable = walkable;
    }

    private void OnDrawGizmos()
    {
        if (gridNodes == null || gridSettings == null) return;
        Gizmos.color = Color.green;
        
        for(int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for(int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                Gizmos.color = node.Walkable ? node.TerrainColor : Color.red;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NoseSize * 0.9f);
            }
        }
    }

}
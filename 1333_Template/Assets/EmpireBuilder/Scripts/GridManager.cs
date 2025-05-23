using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings;
    public GridSettings GridSettings => gridSettings;

    [Header("Terrain List")]
    [SerializeField] private List<TerrainType> terrainTypes = new();

    private PathfindingManager pathfindingManager;
    private GridNode[,] gridNodes;

    [Header("Debug for editor playmode only")]
    [SerializeField] private List<GridNode> AllNode = new();

    public bool IsInitialized { get; private set; } = false;

    private void Start()
    {
        pathfindingManager = GetComponent<PathfindingManager>();
    }

    private void Update()
    {
        // Regenerate grid and notify pathfinder when Space is pressed
        if (Input.GetKeyDown(KeyCode.P))
        {
            InitializeGrid();
            // Notify pathfinding manager to recalc
            if (pathfindingManager != null)
                pathfindingManager.GridUpdated();
        }
    }

    public void InitializeGrid()
    {
        gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];

        for(int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for(int y = 0; y < gridSettings.GridSizeY; y++)
            {
                Vector3 worldPos = gridSettings.UseXYPlane ? new Vector3(x, 0, y) * gridSettings.NodeSize : new Vector3(x, y, 0) * gridSettings.NodeSize;

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
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);
            }
        }
    }

    public GridNode GetNode(int x, int y)
    {
        if (!IsInitialized) InitializeGrid();
        return gridNodes[x, y];
    }

}
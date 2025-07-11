using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings;
    public GridSettings GridSettings => gridSettings;
    public float NodeSize => gridSettings.NodeSize;

    [Header("Terrain List")]
    [SerializeField] private List<TerrainType> terrainTypes = new();

    private PathfindingManager pathfindingManager;
    private GridNode[,] gridNodes;

    [Header("Debug for editor playmode only")]
    [SerializeField] private List<GridNode> AllNode = new();

    public bool IsInitialized { get; private set; } = false;

    private HashSet<GridNode> _reservedNodes = new HashSet<GridNode>();

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
                Vector3 worldPos = gridSettings.UseXZPlane ? new Vector3(x, 0, y) * gridSettings.NodeSize : new Vector3(x, y, 0) * gridSettings.NodeSize;

                TerrainType newTerrain = getRandomTerrain(Random.Range(0,100));
                // TerrainType newTerrain = terrainTypes[Random.Range(0, terrainTypes.Count)]; // Old code

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

    public bool IsNodeReserved(GridNode node)
    {
        return _reservedNodes.Contains(node);
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
        // Ensure the grid is initialized
        if (!IsInitialized) InitializeGrid();

        /* MAYBE NOT WORKING
        // Ensure the coordinates are within grid bounds
        if (x < 0 || x >= gridSettings.GridSizeX || y < 0 || y >= gridSettings.GridSizeY)
            throw new System.IndexOutOfRangeException("Grid node indices out of range");
        */

        // Return the node
        return gridNodes[x, y];
    }

    public GridNode GetNodeFromWorldPosition(Vector3 position)
    {
        /* NOT USING, CAUSE ERROR IN THE SELECTION BOX DRAWER
        // Determine the axes to be used based on the grid orientation
        int x = gridSettings.UseXYPlane ? Mathf.RoundToInt(position.x / gridSettings.NodeSize) : Mathf.RoundToInt(position.x / gridSettings.NodeSize);
        int y = gridSettings.UseXYPlane ? Mathf.RoundToInt(position.z / gridSettings.NodeSize) : Mathf.RoundToInt(position.y / gridSettings.NodeSize);

        // Clamp the coordinates to the grid bounds
        x = Mathf.Clamp(x, 0, gridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSettings.GridSizeY - 1);
        */

        // Determine which axes to use based on grid orientation
        int x = gridSettings.UseXZPlane ? Mathf.RoundToInt(position.x / gridSettings.NodeSize) : Mathf.RoundToInt(position.x / gridSettings.NodeSize);
        int y = gridSettings.UseXZPlane ? Mathf.RoundToInt(position.z / gridSettings.NodeSize) : Mathf.RoundToInt(position.y / gridSettings.NodeSize);
        // Clamp coordinates grid bounds
        x = Mathf.Clamp(x, 0, gridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSettings.GridSizeY - 1);
        // Return the node of clamped coordinates
        return GetNode(x, y);
    }

    private TerrainType getRandomTerrain(int index)
    {
        // Return terrainType "Rock"
        if (index <= 5)
        {
            return terrainTypes[2];
        }

        // Return terrainType "Grass"
        return terrainTypes[0];
    }

    public GridNode? GetRandomWalkableNode()
    {
        int gridWidth = gridSettings.GridSizeX;
        int gridHeight = gridSettings.GridSizeY;

        List<GridNode> walkableNodes = new List<GridNode>();

        // Collect all walkable nodes
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridNode node = gridNodes[x, y];
                if (node.Walkable)
                {
                    walkableNodes.Add(node);
                }
            }
        }

        if (walkableNodes.Count == 0)
        {
            return null; // no valid spawn node
        }

        // Pick one at random
        int index = Random.Range(0, walkableNodes.Count);
        return walkableNodes[index];
    }

    public List<GridNode> FindNearestFreeNodes(GridNode center, int count)
    {
        List<GridNode> result = new List<GridNode>();
        HashSet<GridNode> checkedNodes = new HashSet<GridNode>();
        Queue<GridNode> queue = new Queue<GridNode>();

        // Begin BFS from the center node
        queue.Enqueue(center);
        checkedNodes.Add(center);

        // Continue until queue is empty or desired count is reached
        while (queue.Count > 0 && result.Count < count)
        {
            GridNode node = queue.Dequeue();

            // If this node is walkable and not reserved, add to results
            if (node.Walkable && !IsNodeReserved(node))
                result.Add(node);

            // Enqueue each neighbor that has not yet been checked
            foreach (GridNode neighbor in GetNeighbors(node))
            {
                if (!checkedNodes.Contains(neighbor))
                {
                    checkedNodes.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return result;
    }

    public IEnumerable<GridNode> GetNeighbors(GridNode node)
    {
        // Convert world position to grid indices
        int x = Mathf.RoundToInt(node.WorldPosition.x / gridSettings.NodeSize);
        int y = Mathf.RoundToInt(node.WorldPosition.z / gridSettings.NodeSize);

        // Yield the node above if within bounds
        if (y + 1 < gridSettings.GridSizeY)
            yield return GetNode(x, y + 1);

        // Yield the node below if within bounds
        if (y - 1 >= 0)
            yield return GetNode(x, y - 1);

        // Yield the node to the right if within bounds
        if (x + 1 < gridSettings.GridSizeX)
            yield return GetNode(x + 1, y);

        // Yield the node to the left if within bounds
        if (x - 1 >= 0)
            yield return GetNode(x - 1, y);
    }

}
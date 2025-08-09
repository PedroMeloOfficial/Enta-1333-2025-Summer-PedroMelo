using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings;
    public GridSettings GridSettings => gridSettings;
    [SerializeField] private GameObject rockPrefab;  
    [SerializeField] private GameObject friendlyBuildingPrefab;
    public GameObject FriendlyBase { get; private set; }

    [SerializeField] private List<TerrainType> terrainTypes = new();

    private GridNode[,] gridNodes;

    [Header("Debug for editor playmode only")]
    [SerializeField] private List<GridNode> AllNodes = new();

    public bool IsInitialized { get; private set; } = false;

    public GridNode GetNodeAt(int x, int y)
    {
        if (x >= 0 && x < gridSettings.GridSizeX && y >= 0 && y < gridSettings.GridSizeY)
            return gridNodes[x, y];
        return null;
    }

    public GridNode[,] GetGrid()
    {
        return gridNodes;
    }
    
    public List<GridNode> GetNodesInSquareRange(GridNode center, int range)
    {
        List<GridNode> list = new();
        var gs = GridSettings;
        Vector3 local = transform.InverseTransformPoint(center.WorldPosition);
        int cx = Mathf.RoundToInt(local.x / gs.NodeSize);
        int cy = Mathf.RoundToInt(local.z / gs.NodeSize);
        for(int dx=-range;dx<=range;dx++)
        for(int dy=-range;dy<=range;dy++)
        {
            var n = GetNodeAt(cx+dx, cy+dy);
            if(n!=null) list.Add(n);
        }
        return list;
    }

    private TerrainType GetWeightedRandomTerrain()
    {
        int totalWeight = 0;
        foreach (var terrain in terrainTypes)
            totalWeight += terrain.SpawnWeight;

        int roll = Random.Range(0, totalWeight);
        int runningWeight = 0;

        foreach (var terrain in terrainTypes)
        {
            runningWeight += terrain.SpawnWeight;
            if (roll < runningWeight)
                return terrain;
        }

        return terrainTypes[0]; // fallback
    }
    
    /// <summary>
    /// Converts a world-space point to the corresponding GridNode,
    /// taking this GridManager’s local transform and node size into account.
    /// Returns null if the point is outside the grid bounds.
    /// </summary>
    public GridNode GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        // Convert world coordinates into the GridManager’s local space
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        // Convert local X-Z into grid indices
        int x = Mathf.FloorToInt(localPos.x / gridSettings.NodeSize);
        int y = Mathf.FloorToInt(localPos.z / gridSettings.NodeSize);

        // Use existing bounds-checked lookup
        return GetNodeAt(x, y);
    }
    
    /*public void InitializeGrid()
    {
        gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                Vector3 localPos = gridSettings.UseXZPlane
                    ? new Vector3(x, 0, y) * gridSettings.NodeSize
                    : new Vector3(x, y, 0) * gridSettings.NodeSize;

                Vector3 worldPos = transform.TransformPoint(localPos);

                TerrainType t = GetWeightedRandomTerrain();

                GridNode node = new GridNode
                {
                    Name         = $"Cell_{x}_{y}",
                    WorldPosition= worldPos,
                    Walkable     = t.Walkable,
                    Weight       = t.MovementCost,
                    TerrainColor = t.GizmoColor
                };
                gridNodes[x, y] = node;

                // ─────────────────────────────── spawn rock on initial unwalkables
                if (!t.Walkable && rockPrefab != null)
                {
                    GameObject rock = Instantiate(rockPrefab, worldPos, Quaternion.identity);
                    rock.name = $"Rock_{x}_{y}";
                }
            }
        }
        IsInitialized = true;
    }*/ 
   public void InitializeGrid()
{
    gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];

    int BASE_SIZE = 10;

    // Base will be vertically centered and flush left
    int baseX = 0;
    int baseY = (gridSettings.GridSizeY - BASE_SIZE) / 2;

    for (int x = 0; x < gridSettings.GridSizeX; x++)
    {
        for (int y = 0; y < gridSettings.GridSizeY; y++)
        {
            Vector3 localPos = gridSettings.UseXZPlane
                ? new Vector3(x, 0, y) * gridSettings.NodeSize
                : new Vector3(x, y, 0) * gridSettings.NodeSize;

            Vector3 worldPos = transform.TransformPoint(localPos);

            bool inBaseArea = x >= baseX && x < baseX + BASE_SIZE &&
                              y >= baseY && y < baseY + BASE_SIZE;

            bool isWall = inBaseArea &&
                          (x == baseX || x == baseX + BASE_SIZE - 1 || y == baseY || y == baseY + BASE_SIZE - 1);

            // Leave entrance: remove 2 rocks at the center of the right wall
            bool isEntrance = (x == baseX + BASE_SIZE - 1) && (y >= baseY && y < baseY + BASE_SIZE);

            GridNode node = new GridNode
            {
                Name = $"Cell_{x}_{y}",
                WorldPosition = worldPos,
                Walkable =  isEntrance || !isWall,
                Weight = 1,
                TerrainColor = Color.green
            };
            gridNodes[x, y] = node;

            // Spawn rock on wall tiles (excluding entrance)
            if (isWall && !isEntrance && rockPrefab != null)
            {
                GameObject rock = Instantiate(rockPrefab, worldPos, Quaternion.identity);
                rock.name = $"Rock_{x}_{y}";
            }
        }
    }
    
    /*for (int x = baseX + 1; x < baseX + BASE_SIZE - 1; x++)
    for (int y = baseY + 1; y < baseY + BASE_SIZE - 1; y++)
    {
        GridNode node = GetNodeAt(x, y);
        if (node != null)
            node.Walkable = true;
    }*/

    // ─────── Place friendly building hugging left wall, vertically centered
    Vector3 baseCenter = gridNodes[baseX + 2, baseY + BASE_SIZE / 2 - 1].WorldPosition;
    float lift = gridSettings.NodeSize * 0.5f;
    Vector3 buildingPos = baseCenter + Vector3.up * lift;

    FriendlyBase = Instantiate(friendlyBuildingPrefab, buildingPos, Quaternion.identity);
    

// Block the walkability of the 2x2 area it occupies
    /*for (int dx = 0; dx < 2; dx++)
    for (int dy = 0; dy < 2; dy++)
    {
        GridNode node = GetNodeAt(baseX + 2 + dx, baseY + BASE_SIZE / 2 - 1 + dy);
        if (node != null)
        {
            node.Walkable = false;
            node.Occupant = building;
        }
    }*/
    
    /*// Manually set entrance tiles walkable
    int entranceX = baseX + BASE_SIZE; // Right wall
    int centerY = baseY + BASE_SIZE / 2;

    GridNode node1 = gridNodes[entranceX, centerY];
    GridNode node2 = gridNodes[entranceX, centerY - 1];

    if (node1 != null) node1.Walkable = true;
    if (node2 != null) node2.Walkable = true;*/

    IsInitialized = true;
}

    /*public GridNode GetNodeAt(int x, int y)
    {
        if (x >= 0 && x < gridSettings.GridSizeX && y >= 0 && y < gridSettings.GridSizeY)
            return gridNodes[x, y];
        return null;
    }*/

    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        // Convert world → local → grid indices
        Vector3  local = transform.InverseTransformPoint(node.WorldPosition);
        float    size  = GridSettings.NodeSize;
        int      x     = Mathf.RoundToInt(local.x / size);
        int      y     = Mathf.RoundToInt(local.z / size);

        int[,] dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
        for (int i = 0; i < dirs.GetLength(0); i++)
        {
            int nx = x + dirs[i, 0];
            int ny = y + dirs[i, 1];

            if (nx < 0 || ny < 0 ||
                nx >= GridSettings.GridSizeX || ny >= GridSettings.GridSizeY)
                continue;

            GridNode n = gridNodes[nx, ny];
            if (n.Walkable)
                neighbours.Add(n);
        }
        return neighbours;
    }

    /// Finds the closest *walkable* neighbour of <paramref name="blockedNode"/>  
    /// relative to the unit standing on <paramref name="origin"/>.
    public GridNode GetNearestWalkableNeighbour(GridNode origin, GridNode blockedNode)
    {
        List<GridNode> neigh = GetNeighbours(blockedNode);
        if (neigh.Count == 0) return null;

        GridNode best = null;
        float    bestSq = float.MaxValue;

        foreach (var n in neigh)
        {
            float sq = (n.WorldPosition - origin.WorldPosition).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = n; }
        }
        return best;
    }
    
    private void OnDrawGizmos()
    {
        if(gridNodes == null || gridSettings == null) return;
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
}
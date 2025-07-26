using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private List<GridNode> path = new();
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Reset();
        }
    }
    
    public void Initialise(GridManager gm)
    {
        gridManager = gm;     
    }

    private void Start()
    {
        Reset();
    }
    private void Reset()
    {
        if (!gridManager.IsInitialized)
            gridManager.InitializeGrid();

        Vector2Int start, goal;
        GridNode[,] grid = gridManager.GetGrid();
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        // Find two random walkable points
        do
        {
            start = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        } while (!grid[start.x, start.y].Walkable);

        do
        {
            goal = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        } while (!grid[goal.x, goal.y].Walkable || goal == start);

        path = BFS(start, goal, grid);
        Debug.Log($"Path from {start} to {goal} found with {path?.Count ?? 0} steps.");
    }
    
    public List<GridNode> FindPath(GridNode start, GridNode goal)
    {
        if (!start.Walkable || !goal.Walkable)
        {
            Debug.LogWarning("Start or goal is not walkable.");
            return new List<GridNode>();
        }

        // Convert world positions to grid-space indices
        Vector3   startLocal = gridManager.transform.InverseTransformPoint(start.WorldPosition);
        Vector3   goalLocal  = gridManager.transform.InverseTransformPoint(goal.WorldPosition);
        float     nodeSize   = gridManager.GridSettings.NodeSize;

        int sx = Mathf.Clamp(Mathf.RoundToInt(startLocal.x / nodeSize), 0, gridManager.GridSettings.GridSizeX - 1);
        int sy = Mathf.Clamp(Mathf.RoundToInt(startLocal.z / nodeSize), 0, gridManager.GridSettings.GridSizeY - 1);

        int gx = Mathf.Clamp(Mathf.RoundToInt(goalLocal.x  / nodeSize), 0, gridManager.GridSettings.GridSizeX - 1);
        int gy = Mathf.Clamp(Mathf.RoundToInt(goalLocal.z  / nodeSize), 0, gridManager.GridSettings.GridSizeY - 1);

        Vector2Int startIdx = new Vector2Int(sx, sy);
        Vector2Int goalIdx  = new Vector2Int(gx, gy);

        return BFS(startIdx, goalIdx, gridManager.GetGrid());
    }

    private List<GridNode> BFS(Vector2Int start, Vector2Int goal, GridNode[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        bool[,] visited = new bool[width, height];
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Queue<Vector2Int> queue = new();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == goal)
                break;

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (next.x >= 0 && next.y >= 0 && next.x < width && next.y < height)
                {
                    if (!visited[next.x, next.y] && grid[next.x, next.y].Walkable)
                    {
                        visited[next.x, next.y] = true;
                        cameFrom[next] = current;
                        queue.Enqueue(next);
                    }
                }
            }
        }

        // Reconstruct path
        List<GridNode> resultPath = new();
        if (!cameFrom.ContainsKey(goal)) return null; // no path

        Vector2Int step = goal;
        while (step != start)
        {
            resultPath.Add(grid[step.x, step.y]);
            step = cameFrom[step];
        }
        resultPath.Add(grid[start.x, start.y]);
        resultPath.Reverse();

        return resultPath;
    }

    private void OnDrawGizmos()
    {
        if (path == null) return;

        Gizmos.color = Color.cyan;
        foreach (GridNode node in path)
        {
            Gizmos.DrawSphere(node.WorldPosition, 0.2f);
        }
    }
}
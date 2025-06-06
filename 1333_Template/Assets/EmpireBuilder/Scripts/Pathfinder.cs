using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Pathfinder
{
    private GridManager gridManager;

    public Pathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    private IEnumerable<GridNode> GetNeighbors(GridManager gm, GridNode node)
    {
        int x = Mathf.RoundToInt(node.WorldPosition.x / gm.GridSettings.NodeSize);
        int y = Mathf.RoundToInt(node.WorldPosition.z / gm.GridSettings.NodeSize);

        if (y + 1 < gm.GridSettings.GridSizeY) yield return gm.GetNode(x, y + 1);
        if (y - 1 >= 0) yield return gm.GetNode(x, y - 1);
        if (x + 1 < gm.GridSettings.GridSizeX) yield return gm.GetNode(x + 1, y);
        if (x - 1 >= 0) yield return gm.GetNode(x - 1, y);
    }

    public List<GridNode> FindPath(Vector3 start, Vector3 end)
    {
        // Convert the start position to the closest grid node
        GridNode startNode = gridManager.GetNodeFromWorldPosition(start);
        // Convert the end position to the closest grid node
        GridNode endNode = gridManager.GetNodeFromWorldPosition(end);
        return FindPath(start, end);
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> openSet = new List<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        openSet.Add(start);
        gScore[start] = 0f;
        fScore[start] = HScore(start, goal);

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        int maxX = gridManager.GridSettings.GridSizeX;
        int maxY = gridManager.GridSettings.GridSizeY;

        while (openSet.Count > 0)
        {
            Vector2Int current = GetLowestFScore(openSet, fScore);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;

                if (neighbor.x < 0 || neighbor.x >= maxX || neighbor.y < 0 || neighbor.y >= maxY)
                    continue;

                if (closedSet.Contains(neighbor))
                    continue;

                GridNode node = gridManager.GetNode(neighbor.x, neighbor.y);

                if (!node.Walkable)
                    continue;

                float tentativeG = gScore[current] + node.Weight;

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeG >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeG;
                fScore[neighbor] = tentativeG + HScore(neighbor, goal);
            }
        }

        return new List<Vector2Int>();
    }

    private Vector2Int GetLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int bestNode = openSet[0];
        float bestScore = fScore.GetValueOrDefault(bestNode, float.MaxValue);

        foreach (Vector2Int node in openSet)
        {
            float score = fScore.GetValueOrDefault(node, float.MaxValue);
            if (score < bestScore)
            {
                bestScore = score;
                bestNode = node;
            }
        }
        return bestNode;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    private float HScore(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private int Heuristic(GridNode a, GridNode b)
    {
        float dx = Mathf.Abs(a.WorldPosition.x - b.WorldPosition.x);
        float dz = Mathf.Abs(a.WorldPosition.z - b.WorldPosition.z);
        return Mathf.RoundToInt(dx + dz);
    }

    public List<Vector2Int> FindPathWithNodes(GridNode start, GridNode end, int unitWidth, int unitHeight)
    {
        PriorityQueue<GridNode> openSet = new PriorityQueue<GridNode>();
        Dictionary<GridNode, int> costSoFar = new Dictionary<GridNode, int>();
        Dictionary<GridNode, GridNode> cameFrom = new Dictionary<GridNode, GridNode>();

        openSet.Enqueue(start, 0);
        costSoFar[start] = 0;
        cameFrom[start] = start;

        while (openSet.Count > 0)
        {
            GridNode current = openSet.Dequeue();

            if (current.Equals(end))
                break;

            foreach (GridNode neighbor in GetNeighbors(gridManager, current))
            {
                if (!IsAreaWalkable(gridManager, neighbor, unitWidth, unitHeight))
                    continue;

                int newCost = costSoFar[current] + neighbor.Weight;
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    int priority = newCost + Heuristic(neighbor, end);
                    openSet.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(end))
            return new List<Vector2Int>();

        List<GridNode> nodePath = new List<GridNode>();
        GridNode pathNode = end;
        while (!pathNode.Equals(start))
        {
            nodePath.Add(pathNode);
            pathNode = cameFrom[pathNode];
        }
        nodePath.Add(start);
        nodePath.Reverse();

        List<Vector2Int> path = new List<Vector2Int>();
        float nodeSize = gridManager.GridSettings.NodeSize;
        foreach (GridNode node in nodePath)
        {
            int x = Mathf.RoundToInt(node.WorldPosition.x / nodeSize);
            int y = Mathf.RoundToInt(node.WorldPosition.z / nodeSize);
            path.Add(new Vector2Int(x, y));
        }

        return path;
    }

    private bool IsAreaWalkable(GridManager gridManager, GridNode node, int width, int height)
    {
        float nodeSize = gridManager.GridSettings.NodeSize;
        int baseX = Mathf.RoundToInt(node.WorldPosition.x / nodeSize);
        int baseY = Mathf.RoundToInt(node.WorldPosition.z / nodeSize);

        for (int a = 0; a < width; a++)
        {
            for (int b = 0; b < height; b++)
            {
                int aa = baseX + a;
                int bb = baseY + b;

                if (aa < 0 || aa >= gridManager.GridSettings.GridSizeX ||
                    bb < 0 || bb >= gridManager.GridSettings.GridSizeY)
                    return false;

                if (!gridManager.GetNode(aa, bb).Walkable)
                    return false;
            }
        }

        return true;
    }
}
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
}
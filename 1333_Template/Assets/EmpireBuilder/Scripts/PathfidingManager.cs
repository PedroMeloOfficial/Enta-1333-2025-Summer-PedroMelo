using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class PathfindingManager : MonoBehaviour
{
    [Header("Pathfinding Settings:")]
    public Vector2Int startCoordinates;
    public Vector2Int goalCoordinates;
    public bool showPath = true;

    private List<Vector2Int> path = new List<Vector2Int>();
    private GridManager gridManager;
    private Pathfinder pathfinder;

    private void Awake()
    {
        gridManager = GetComponent<GridManager>();
        pathfinder = new Pathfinder(gridManager);
    }
    private void Start()
    {
        if (gridManager != null && gridManager.IsInitialized && showPath)
        {
            path = pathfinder.FindPath(startCoordinates, goalCoordinates);
        }
    }

    private void OnValidate()
    {
        if (gridManager != null && gridManager.IsInitialized && showPath)
        {
            path = pathfinder.FindPath(startCoordinates, goalCoordinates);
        }
    }

    public void GridUpdated()
    {
        if (gridManager != null && gridManager.IsInitialized && showPath)
        {
            path = pathfinder.FindPath(startCoordinates, goalCoordinates);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showPath || path == null || gridManager == null || !gridManager.IsInitialized)
            return;

        Gizmos.color = Color.red;
        float size = gridManager.GridSettings.NodeSize * 0.3f;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int coord = path[i];
            GridNode node = gridManager.GetNode(coord.x, coord.y);
            Gizmos.DrawCube(node.WorldPosition + Vector3.up * 0.1f, Vector3.one * size);

            if (i > 0)
            {
                Vector2Int prev = path[i - 1];
                GridNode prevNode = gridManager.GetNode(prev.x, prev.y);
                Gizmos.DrawLine(prevNode.WorldPosition + Vector3.up * 0.1f,
                                node.WorldPosition + Vector3.up * 0.1f);
            }
        }
    }
}
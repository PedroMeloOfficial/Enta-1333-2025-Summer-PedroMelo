// Assets/Scripts/Units/UnitMover.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(UnitAI))]
public class UnitMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed      = 3f; // world units/sec
    [SerializeField] private float repathInterval = 1f; // seconds
    [SerializeField] private float turnSpeedDeg   = 360f; // ★ Rotate – deg/sec

    GridManager  grid;
    Pathfinding  pathfinding;
    UnitAI       ai;

    List<GridNode> currentPath = new();
    int   pathIndex;
    float repathTimer;

    void Awake()
    {
        ai          = GetComponent<UnitAI>();
        grid        = ai.GridManager;
        pathfinding = ai.Pathfinding;
    }

    public void Inject(GridManager gm, Pathfinding pf)
    {
        grid        = gm;
        pathfinding = pf;
        repathTimer = 0f;   // force an immediate first path
    }

    void Update()
    {
        // 1. Ask AI if we need to repath
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            TryRepath();
            repathTimer = repathInterval;
        }

        // 2. Follow path
        if (currentPath is { Count: > 0 } && pathIndex < currentPath.Count)
        {
            Vector3 nextXZ = currentPath[pathIndex].WorldPosition;
            nextXZ.y = transform.position.y;                // preserve current Y

            // Rotate – face next waypoint (Y-axis only)
            Vector3 lookDir = nextXZ - transform.position;
            lookDir.y = 0f;                                 // lock to ground plane
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    turnSpeedDeg * Time.deltaTime
                );
            }

            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, nextXZ, step);

            if (Vector3.Distance(transform.position, nextXZ) < 0.05f)
                pathIndex++;

            Debug.DrawLine(transform.position, nextXZ, Color.yellow, 0.1f);
        }
    }

    void TryRepath()
    {
        if (grid == null || pathfinding == null) return;

        GridNode start = grid.GetNodeFromWorldPosition(transform.position);
        GridNode goal  = ai.GetDestination();
        if (goal == null || goal == start) return;

        currentPath = pathfinding.FindPath(start, goal);
        pathIndex   = currentPath != null && currentPath.Count > 1 ? 1 : 0;
    }

    public void MoveTo(GridNode node)
    {
        if (node == null) return;

        currentPath = pathfinding.FindPath(grid.GetNodeFromWorldPosition(transform.position), node);
        pathIndex = 0;
    }


}

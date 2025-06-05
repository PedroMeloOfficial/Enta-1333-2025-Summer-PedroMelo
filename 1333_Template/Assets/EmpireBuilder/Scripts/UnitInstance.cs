using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstance : UnitBase, ISelectable
{
    [Header("Prefab Stuff")]
    [SerializeField] private Transform animationParent;

    // Private property
    private GameObject animatedUnit;
    private Pathfinder pathfinder;
    private Animator animator;
    private List<GridNode> currentPath = new();
    private int pathIndex = 0;
    private Vector3? targetWorldPosition = null;
    private bool isMoving = false;

    // Public property
    public bool IsMoving => isMoving;
    public List<GridNode> CurrentPath => currentPath;

    private void Update()
    {
        // If not moving or no path, do nothing
        if (!isMoving || currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
            return;

        // Get the next waypoint
        Vector3 nextWaypoint = currentPath[pathIndex].WorldPosition;
        // Move towards the waypoint
        Vector3 direction = (nextWaypoint - transform.position).normalized;
        float step = unitType.MoveSpeed + Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, nextWaypoint, step);

        // Check if reached the waypoint
        if (Vector3.Distance(transform.position, nextWaypoint) < 0.05f)
        {
            pathIndex++;
            // If reached the end of the path stop moving
            if (pathIndex >= currentPath.Count)
            {
                isMoving = false;
            }
        }
    }

    public void Initialize(Pathfinder _pathfinder, UnitType _unitType)
    {
        pathfinder = _pathfinder;
        unitType = _unitType;

        animatedUnit = Instantiate(unitType.Prefab, animationParent);

        animator = animatedUnit.GetComponent<Animator>();
    }

    public override void MoveTo(GridNode targetNode)
    {
        SetTarget(targetNode);
    }

    // Sets the node as the new movement target for the unit
    public void SetTarget(GridNode node)
    {
        SetTarget(node.WorldPosition);
    }

    // Sets the worldPosition as the new movement target for the unit
    public void SetTarget(Vector3 worldPosition)
    {
        // Store the target's position
        targetWorldPosition = worldPosition;
        // Request a path from the Pathfinder
        currentPath = pathfinder.FindPath(transform.position, worldPosition);
        pathIndex = 0;
        isMoving = currentPath != null && currentPath.Count > 1;
    }

    public void OnSelect()
    {
        
    }

    public void OnDeselect()
    {

    }

    public string GetLabel()
    {
        return unitType.name;
    }

}
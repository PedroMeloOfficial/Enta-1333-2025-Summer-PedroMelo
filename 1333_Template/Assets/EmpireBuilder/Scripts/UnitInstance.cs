using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstance : UnitBase, ISelectable
{
    [Header("Prefab Stuff")]
    [SerializeField] private Transform animationParent;
    [SerializeField] protected float moveSpeed = 4f;
    [SerializeField] protected float rotationSpeed = 280f;

    // Private property
    private GameObject animatedUnit;
    private Animator animator;
    private List<GridNode> currentPath = new();
    protected List<Vector2Int> currentPath2d = null;
    private int pathIndex = 0;
    private Vector3? targetWorldPosition = null;
    private bool isMoving = false;
    private Team unitTeam;
    protected UnitState state = UnitState.Idle;
    protected int nextPathIndex = 0;

    // Public property
    public static bool ShowPathGizmos = false;
    public UnitType UnitType => unitType;
    public bool IsMoving => isMoving;
    public Team UnitTeam => unitTeam;
    public List<GridNode> CurrentPath => currentPath;

    // Catched references
    protected GridManager gridManager;
    protected Pathfinder pathfinder;

    private void Update()
    {
        if (state == UnitState.Moving)
        {
            HandleMovement();
        }

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

    public virtual void Initialize(UnitType _unitType, GridManager _gridManager, Pathfinder _pathfinder, Team _team)
    {
        unitType = _unitType;
        moveSpeed = _unitType.MoveSpeed;
        gridManager = _gridManager;
        pathfinder = _pathfinder;
        unitTeam = _team;

        // Apply team material
        Material material = unitType.GetTeamMaterial(unitTeam);
        if (material != null)
        {
            UnitTeamRef headRef = GetComponent<UnitTeamRef>();
            if (headRef != null && headRef.headRenderer != null)
            {
                headRef.headRenderer.material = material;
            }
        }
    }

    /* public void Initialize(Pathfinder _pathfinder, UnitType _unitType)
    {
        pathfinder = _pathfinder;
        unitType = _unitType;

        animatedUnit = Instantiate(unitType.Prefab, animationParent);

        animator = animatedUnit.GetComponent<Animator>();
    } */

    public override void MoveTo(GridNode targetNode)
    {
        if (gridManager == null || pathfinder == null)
        {
            Debug.LogWarning($"SpearMan.MoveTo: Missing GridManager or AStarPathfinder on {name}.");
            return;
        }

        // Determine the current grid node based on world position
        GridNode currentNode = gridManager.GetNodeFromWorldPosition(transform.position);
        if (currentNode.Equals(default(GridNode)))
        {
            Debug.LogWarning("UnitInstance: Could not identify the current GridNode");
            return;
        }

        // Use FindPathWithNodes to get a list of Vector2Int
        List<Vector2Int> path = pathfinder.FindPathWithNodes(
            currentNode,
            targetNode,
            Width,
            Height
        );

        // Log and return for no path found
        if (path == null || path.Count == 0)
        {
            Debug.Log($"UnitInstance: No path found");
            return;
        }

        // Assign a new path and change the unit state
        currentPath2d = path;
        nextPathIndex = 0;
        state = UnitState.Moving;
    }

    protected virtual void HandleMovement()
    {
        if (currentPath2d == null || nextPathIndex >= currentPath2d.Count)
        {
            return;
        }

        // Determine next target node and the world position
        Vector2Int nextCoords = currentPath2d[nextPathIndex];
        GridNode nextNode = gridManager.GetNode(nextCoords.x, nextCoords.y);
        Vector3 nextWorldPos = nextNode.WorldPosition + Vector3.up * 0.1f;

        // Compute horizontal direction to the next waypoint
        Vector3 direction = nextWorldPos - transform.position;
        direction.y = 0f; // zero out vertical component
        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            // Compute target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            // Rotate to that direction
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float angleDifference = Vector3.Angle(transform.forward, direction);

        transform.position = Vector3.MoveTowards(
            transform.position,
            nextWorldPos,
            moveSpeed * Time.deltaTime
        );

        // Check if reached the waypoint
        if (Vector3.Distance(transform.position, nextWorldPos) < 0.01f)
        {
            nextPathIndex++;
            if (nextPathIndex >= currentPath2d.Count)
            {
                // Arrived at final destination
                state = UnitState.Idle;
                // OnArrivedAtDestination(state); NOT WKORKING +++++++
            }
        }
    }

    // OLD CODE NOT WORKING
    /* public override void MoveTo(GridNode targetNode)
    {
        SetTarget(targetNode);
    } */

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

    /* NOT WORKING
    protected virtual void OnArrivedAtDestination(UnitState unitState)
    {
        if (animatoHandler != null)
            animatoHandler.OnStateChanged(unitState);
    }
    */

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
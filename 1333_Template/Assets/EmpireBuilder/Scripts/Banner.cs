using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Banner : MonoBehaviour, ISelectable
{
    [Header("Dependencies")]
    [Tooltip("Reference to the GridManager for snapping to grid.")]
    [SerializeField] private GridManager _gridManager;

    private Camera _mainCamera;
    private bool _isDragging = false;
    private Quaternion _originalRotation;
    private Plane _groundPlane;
    private GridNode _occupiedNode;

    public static bool IsAnyDragging { get; private set; } = false;

    public static event System.Action<Vector3> BannerMoved;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _originalRotation = transform.rotation;
        _groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (_gridManager == null)
            Debug.LogError("Banner: GridManager is not assigned.");
    }

    private void Start()
    {
        // Occupy the cell we start in
        GridNode startNode = _gridManager.GetNodeFromWorldPosition(transform.position);
        OccupyNode(startNode);
    }

    private void OnMouseDown()
    {
        _isDragging = true;
        IsAnyDragging = true;
    }

    private void OnMouseUp()
    {
        if (!_isDragging) return;

        // Snap to nearest valid node
        GridNode node = PlaceAtCursor();
        if (node != null)
            OccupyNode(node);

        // End drag state
        _isDragging = false;
        IsAnyDragging = false;

        // Notify listeners of new banner position
        BannerMoved?.Invoke(transform.position);
    }

    private void Update()
    {
        if (!_isDragging) return;

        // Follow mouse on ground plane
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (_groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            GridNode node = _gridManager.GetNodeFromWorldPosition(hitPoint);
            if (node != null && node.Walkable)
            {
                transform.position = node.WorldPosition;
                transform.rotation = _originalRotation;
            }
        }
    }

    private GridNode PlaceAtCursor()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (_groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            GridNode node = _gridManager.GetNodeFromWorldPosition(hitPoint);
            if (node != null && node.Walkable)
            {
                transform.position = node.WorldPosition;
                transform.rotation = _originalRotation;
                return node;
            }
        }
        return null;
    }

    private void OccupyNode(GridNode node)
    {
        if (_occupiedNode != null)
            SetNodeWalkable(_occupiedNode, true);

        _occupiedNode = node;
        SetNodeWalkable(node, false);
    }

    private void SetNodeWalkable(GridNode node, bool walkable)
    {
        int x = Mathf.RoundToInt(node.WorldPosition.x / _gridManager.GridSettings.NodeSize);
        int y = Mathf.RoundToInt(
            _gridManager.GridSettings.UseXZPlane
                ? node.WorldPosition.z / _gridManager.GridSettings.NodeSize
                : node.WorldPosition.y / _gridManager.GridSettings.NodeSize
        );
        _gridManager.SetWalkable(x, y, walkable);
    }

    // ISelectable implementation (no UI for banners)
    public void OnSelected() { }
    public void OnDeselected() { }
}

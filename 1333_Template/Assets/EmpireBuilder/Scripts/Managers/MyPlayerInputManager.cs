using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyPlayerInputManager : MonoBehaviour
{
    private Camera cam;
    private GridManager gridManager;
    private UnitManager unitManager;
    private SelectionBoxDrawer selectionBoxDrawer;
    private CameraController camController;

    [SerializeField] private float minDragSize = 3f;

    private readonly List<UnitBase> selectedUnits = new();

    // Track any ISelectable
    private readonly List<ISelectable> selected = new List<ISelectable>();

    public void Initialize(Camera _cam, GridManager _gridManager, UnitManager _unitManager)
    {
        cam = _cam;
        gridManager = _gridManager;
        unitManager = _unitManager;
        selectionBoxDrawer = GetComponent<SelectionBoxDrawer>();
        selectionBoxDrawer.minDragSize = minDragSize;
        camController = GetComponent<CameraController>();

        camController.Initialize(cam);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            UnitInstance.ShowPathGizmos = !UnitInstance.ShowPathGizmos;

        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            selectionBoxDrawer.BeginDrag(Mouse.current.position.ReadValue());

        if (selectionBoxDrawer.IsDragging)
            selectionBoxDrawer.UpdateDrag(Mouse.current.position.ReadValue());

        if (Input.GetMouseButtonUp(0) && selectionBoxDrawer.IsDragging)
        {
            selectionBoxDrawer.EndDrag(Mouse.current.position.ReadValue());
            if (selectionBoxDrawer.DragDistance < minDragSize)
                SingleClickSelect(selectionBoxDrawer.DragEnd);
            else
                DragSelect(selectionBoxDrawer.DragStart, selectionBoxDrawer.DragEnd);
        }

        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
            CommandSelectedUnits();
    }

    private void SingleClickSelect(Vector2 screenPos)
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            ISelectable selectable = hit.collider.GetComponentInParent<ISelectable>();
            if (selectable is UnitInstance unit && unit.UnitTeam == Team.Player)
            {
                ClearSelection();
                AddToSelection(unit);
                return;
            }
        }

        ClearSelection();
    }

    private void DragSelect(Vector2 start, Vector2 end)
    {
        if (cam == null || unitManager == null) return;

        Rect rect = selectionBoxDrawer.GetScreenRect(start, end);
        ClearSelection();

        foreach (UnitInstance unit in unitManager.AllUnits)
        {
            if (unit.UnitTeam != Team.Player) continue;

            Vector3 screenPoint = cam.WorldToScreenPoint(unit.transform.position);
            if (screenPoint.z < 0) continue;

            Vector2 guiPoint = new(screenPoint.x, Screen.height - screenPoint.y);
            if (rect.Contains(guiPoint))
                AddToSelection(unit);
        }
    }

    private void CommandSelectedUnits()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out var enter)) return;

        Vector3 hitPoint = ray.GetPoint(enter);
        GridNode targetNode = gridManager.GetNodeFromWorldPosition(hitPoint);
        if (!targetNode.Walkable) return;

        // Gather selected units
        var units = new List<UnitInstance>();
        foreach (var sel in selected)
            if (sel is UnitInstance unit)
                units.Add(unit);

        // 1) Free all start cells so no unit blocks pathfinding
        foreach (var u in units)
        {
            GridNode startNode = gridManager.GetNodeFromWorldPosition(u.transform.position);
            startNode.Walkable = true;
        }

        // 2) Find nearest free nodes around the target for each unit
        var assignedNodes = gridManager.FindNearestFreeNodes(targetNode, units.Count);

        // 3) Issue movement commands
        for (int i = 0; i < units.Count; i++)
        {
            UnitInstance u = units[i];
            var destNode = assignedNodes[i];
            u.SetReservedDestination(destNode);
            u.MoveTo(destNode);
        }
    }

    private void AddToSelection(UnitInstance unit)
    {
        if (selectedUnits.Contains(unit)) return;

        selectedUnits.Add(unit);
        if (unit.TryGetComponent(out UnitTeamRef head) && head.headRenderer != null)
            head.headRenderer.material.color = Color.yellow;
    }

    private void ClearSelection()
    {
        foreach (UnitInstance unit in selectedUnits)
        {
            if (unit.TryGetComponent(out UnitTeamRef head) && head.headRenderer != null)
                head.headRenderer.material = unit.UnitType.GetTeamMaterial(unit.UnitTeam);
        }

        selectedUnits.Clear();
    }

}
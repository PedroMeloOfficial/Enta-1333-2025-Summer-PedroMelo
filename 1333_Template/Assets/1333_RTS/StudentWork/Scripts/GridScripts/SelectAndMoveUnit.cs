using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Diagnostics;

public class SelectAndMoveUnit : MonoBehaviour
{
    private GridManager gridManger;

    public LayerMask unitLayer;
    public LayerMask gridLayer;
    public Texture2D selectionTexture;

    private Vector2 startMousePosition;
    private Vector2 endMousePosition;
    private bool isDragging = false;

    private List<UnitMover> selectedUnits = new List<UnitMover>();

    public void Initialize(GridManager _gridManger)
    {
        gridManger = _gridManger;
    }

    void Update()
    {
        HandleLeftMouse();
        HandleRightMouse();
    }

    void HandleLeftMouse()
    {
        // Start drag
        if (Input.GetMouseButtonDown(0))
        {
            startMousePosition = Input.mousePosition;
            isDragging = true;
        }

        // End drag
        if (Input.GetMouseButtonUp(0))
        {
            endMousePosition = Input.mousePosition;
            isDragging = false;
            SelectUnitsInRectangle(startMousePosition, endMousePosition);
        }
    }

    void HandleRightMouse()
    {
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);

                // Get the GridNode at the clicked world position
                GridNode node = gridManger.GetNodeFromWorldPosition(worldPoint);

                if (node != null && node.Walkable)
                {
                    foreach (UnitMover unit in selectedUnits)
                    {
                        unit.MoveTo(node);
                    }
                }
                else
                {
                    Debug.Log("No walkable node at this position.");
                }
            }
        }
    }


    void SelectUnitsInRectangle(Vector2 screenStart, Vector2 screenEnd)
    {
        selectedUnits.Clear();

        // Build a ray from both corners of the selection box
        Ray ray1 = Camera.main.ScreenPointToRay(screenStart);
        Ray ray2 = Camera.main.ScreenPointToRay(screenEnd);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray1, out float dist1) && groundPlane.Raycast(ray2, out float dist2))
        {
            Vector3 worldStart = ray1.GetPoint(dist1);
            Vector3 worldEnd = ray2.GetPoint(dist2);

            // Calculate box center and size
            Vector3 center = (worldStart + worldEnd) / 2f;
            Vector3 size = new Vector3(
                Mathf.Abs(worldStart.x - worldEnd.x),
                5f, // Height - make sure it covers units regardless of Y
                Mathf.Abs(worldStart.z - worldEnd.z)
            );

            // Overlap box to get units
            Collider[] hits = Physics.OverlapBox(center, size / 2f, Quaternion.identity, unitLayer);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Ally"))
                {
                    UnitMover mover = hit.GetComponent<UnitMover>();
                    if (mover != null)
                    {
                        selectedUnits.Add(mover);
                    }
                }
            }
        }

        // If none selected, cancel selection mode
        if (selectedUnits.Count == 0)
        {
            Debug.Log("No friendly units selected. Clearing selection.");
        }
    }

    void OnGUI()
    {
        if (isDragging)
        {
            Rect rect = Utils.GetScreenRect(startMousePosition, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 1f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, Color.blue);
        }
    }
}

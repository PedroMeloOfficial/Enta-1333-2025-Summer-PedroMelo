using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildingPlacementManager : MonoBehaviour
{
    [SerializeField] private Material ghostValidMaterial;
    [Tooltip("Semi-transparent red material for invalid placement")]
    [SerializeField] private Material ghostInvalidMaterial;

    private Camera mainCamera;
    private BuildingData currentBuildingData;
    private GameObject previewInstance;
    private Quaternion previewBaseRotation;
    private int currentYRotation = 0;

    private GridManager gridManager;
    private UnitManager unitManager;
    private ResourceManager resourceManager;

    public void Initialize(GridManager _gridManager, UnitManager _unitManager, ResourceManager _resourceManager)
    {
        gridManager = _gridManager;
        unitManager = _unitManager;
        resourceManager = _resourceManager;

        if (_gridManager == null)
            Debug.LogError("BuildingPlacementManager: GridManager not assigned");
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("BuildingPlacementManager: MainCamera not assinged");
    }

    private void Update()
    {
        if (previewInstance == null)
            return;

        // Rotate preview on middle mouse click
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            currentYRotation = (currentYRotation + 90) % 360;
            ApplyRotation(previewInstance.transform, previewBaseRotation, currentYRotation);
        }

        // Always update preview position
        if (TryGetMouseWorldPosition(out Vector3 hitPoint))
        {
            bool canPlace = CanPlace(currentBuildingData, hitPoint, currentYRotation, out Vector3 snapPos);
            previewInstance.transform.position = snapPos;
            ApplyGhostMaterial(previewInstance, canPlace ? ghostValidMaterial : ghostInvalidMaterial);

            // Place on left-click if valid and not over blocking UI
            if (canPlace && Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
            {
                PlaceRealBuilding(hitPoint);
                return;
            }
        }

        // Cancel on right-click if not over blocking UI
        if (Mouse.current.rightButton.wasPressedThisFrame && !IsPointerOverUI())
        {
            CancelPlacement();
        }
    }

    private bool CanPlace(BuildingData data, Vector3 worldPos, int rot, out Vector3 snap)
    {
        Vector2Int idx = GetBaseIndices(worldPos);
        snap = CalculateSnapPosition(idx, data, rot);
        return IsAreaWalkable(idx, GetRotatedSize(data, rot));
    }

    private Vector2Int GetBaseIndices(Vector3 worldPos)
    {
        var node = gridManager.GetNodeFromWorldPosition(worldPos);
        float sz = gridManager.GridSettings.NodeSize;
        return new Vector2Int(
            Mathf.RoundToInt(node.WorldPosition.x / sz),
            Mathf.RoundToInt(node.WorldPosition.z / sz)
        );
    }

    public void StartPlacement(BuildingData buildingData)
    {
        if (previewInstance != null)
            Destroy(previewInstance);

        currentBuildingData = buildingData;
        currentYRotation = 0;
        CreatePreview();
    }

    private void CancelPlacement()
    {
        if (previewInstance != null)
            Destroy(previewInstance);

        previewInstance = null;
        currentYRotation = 0;
    }

    private void PlaceRealBuilding(Vector3 worldPosition)
    {
        var realGO = Instantiate(currentBuildingData.BuildingPrefab);
        var realBase = realGO.GetComponent<BuildingBase>();
        realBase.buildingData = currentBuildingData;
        realBase.team = Team.Player;
        realBase.ApplyTeamMaterial();

        realGO.transform.rotation = previewInstance.transform.rotation;

        Vector2Int baseIdx = GetBaseIndices(worldPosition);
        Vector2Int footprint = GetRotatedSize(currentBuildingData, currentYRotation);

        if (realBase is BuildingGate gate)
            gate.InitializePlacement(baseIdx, footprint, gridManager);

        // Injection for Barrack:
        if (realBase is BuildingBarrack barrack)
            barrack.Initialize(unitManager, resourceManager, gridManager);

        // If it produces resources, inject manager
        if (realBase is BuildingResource br && resourceManager != null)
            br.Initialize(resourceManager);

        Vector3 snapPos = CalculateSnapPosition(baseIdx, currentBuildingData, currentYRotation);
        realGO.transform.position = snapPos;
        MarkAreaOccupied(baseIdx, footprint, false);

        Destroy(previewInstance);
        previewInstance = null;
        CreatePreview();
    }

    private void CreatePreview()
    {
        previewInstance = Instantiate(currentBuildingData.BuildingModel);
        previewBaseRotation = previewInstance.transform.rotation;
        ApplyRotation(previewInstance.transform, previewBaseRotation, currentYRotation);
        ApplyGhostMaterial(previewInstance, ghostInvalidMaterial);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var res in results)
            if (!res.gameObject.CompareTag("UIIgnore"))
                return true;

        return false;
    }

    private void ApplyRotation(Transform target, Quaternion baseRot, int yRot)
    {
        Vector3 e = baseRot.eulerAngles;
        target.rotation = Quaternion.Euler(e.x, e.y + yRot, e.z);
    }

    private void ApplyGhostMaterial(GameObject instance, Material mat)
    {
        foreach (var r in instance.GetComponentsInChildren<Renderer>())
            r.material = mat;
    }

    private Vector3 CalculateSnapPosition(Vector2Int idx, BuildingData data, int rot)
    {
        float sz = gridManager.GridSettings.NodeSize;
        var fp = GetRotatedSize(data, rot);
        float w = fp.x * sz, d = fp.y * sz;
        float y = gridManager.GetNodeFromWorldPosition(
            new Vector3(idx.x * sz, 0, idx.y * sz)
        ).WorldPosition.y;
        return new Vector3(
            idx.x * sz + (w - sz) * 0.5f,
            y,
            idx.y * sz + (d - sz) * 0.5f
        );
    }

    private Vector2Int GetRotatedSize(BuildingData data, int rot) =>
        (rot % 180 == 0)
            ? new Vector2Int(data.SizeX, data.SizeZ)
            : new Vector2Int(data.SizeZ, data.SizeX);

    private bool IsAreaWalkable(Vector2Int idx, Vector2Int fp)
    {
        for (int x = 0; x < fp.x; x++)
            for (int y = 0; y < fp.y; y++)
            {
                var n = gridManager.GetNode(idx.x + x, idx.y + y);
                if (n == null || !n.Walkable)
                    return false;
            }
        return true;
    }

    private void MarkAreaOccupied(Vector2Int idx, Vector2Int fp, bool walkable)
    {
        for (int x = 0; x < fp.x; x++)
            for (int y = 0; y < fp.y; y++)
                gridManager.SetWalkable(idx.x + x, idx.y + y, walkable);
    }

    private bool TryGetMouseWorldPosition(out Vector3 pos)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float d))
        {
            pos = ray.GetPoint(d);
            return true;
        }
        pos = Vector3.zero;
        return false;
    }

}
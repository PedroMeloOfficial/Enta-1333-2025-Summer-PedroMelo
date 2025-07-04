using System.Collections.Generic;
using UnityEngine;

// gate can open and close toggling the walkability
[RequireComponent(typeof(Renderer))]
public class BuildingGate : BuildingBase
{
    [SerializeField] private SkinnedMeshRenderer[] _skinnedRenderers;
    [SerializeField] private Animator _animator;

    public enum GateState { Closed, Opening, Open, Closing }
    private GateState _currentState = GateState.Closed;

    private Vector2Int _placementBase;
    private Vector2Int _placementFootprint;
    private GridManager _gridManager;
    private Vector2Int[] _centerOffsets;

    private static readonly int OpenTrigger = Animator.StringToHash("Open");
    private static readonly int CloseTrigger = Animator.StringToHash("Close");

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) OpenGate();
        if (Input.GetKeyDown(KeyCode.B)) CloseGate();
    }

    // Cache base Awake logic and ensure renderer/animator references
    protected override void Awake()
    {
        base.Awake();
        if (_skinnedRenderers == null || _skinnedRenderers.Length == 0)
            _skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    public override void ApplyTeamMaterial()
    {
        base.ApplyTeamMaterial();
        var mat = teamMaterials[(int)team];
        foreach (var smr in _skinnedRenderers)
            if (smr != null)
                smr.material = mat;
    }

    public void InitializePlacement(Vector2Int baseIndices, Vector2Int footprint, GridManager gridManager)
    {
        _placementBase = baseIndices;
        _placementFootprint = footprint;
        _gridManager = gridManager;

        bool rotated = footprint.x != buildingData.SizeX;
        var offsets = new List<Vector2Int>();

        if (!rotated)
        {
            int half = footprint.x / 2;
            for (int y = 0; y < footprint.y; y++)
            {
                offsets.Add(new Vector2Int(half - 2, y));
                offsets.Add(new Vector2Int(half - 1, y));
                offsets.Add(new Vector2Int(half, y));
                offsets.Add(new Vector2Int(half + 1, y));
            }
        }
        else
        {
            int half = footprint.y / 2;
            for (int x = 0; x < footprint.x; x++)
            {
                offsets.Add(new Vector2Int(x, half - 2));
                offsets.Add(new Vector2Int(x, half - 1));
                offsets.Add(new Vector2Int(x, half));
                offsets.Add(new Vector2Int(x, half + 1));
            }
        }

        _centerOffsets = offsets.ToArray();
    }

    public void OpenGate()
    {
        if (_currentState == GateState.Opening || _currentState == GateState.Open)
            return;
        _currentState = GateState.Opening;
        _animator?.SetTrigger(OpenTrigger);
        OnGateGridOpened();
    }

    public void CloseGate()
    {
        if (_currentState == GateState.Closing || _currentState == GateState.Closed)
            return;
        _currentState = GateState.Closing;
        _animator?.SetTrigger(CloseTrigger);
        OnGateGridClosed();
    }

    public void OnGateGridOpened()
    {
        _currentState = GateState.Open;
        if (_gridManager == null) return;
        foreach (var offset in _centerOffsets)
        {
            int x = _placementBase.x + offset.x;
            int y = _placementBase.y + offset.y;
            _gridManager.SetWalkable(x, y, true);
            Debug.Log($"[Gate] Opened cell ({x},{y}) ? walkable");
        }
    }

    public void OnGateGridClosed()
    {
        _currentState = GateState.Closed;
        if (_gridManager == null) return;
        foreach (var offset in _centerOffsets)
        {
            int x = _placementBase.x + offset.x;
            int y = _placementBase.y + offset.y;
            _gridManager.SetWalkable(x, y, false);
            Debug.Log($"[Gate] Closed cell ({x},{y}) ? blocked");
        }
    }

    public override void OnSelected()
    {
        foreach (var smr in _skinnedRenderers)
            if (smr != null)
                smr.material.color = Color.gray;
    }

    public override void OnDeselected()
    {
        ApplyTeamMaterial();
    }
}
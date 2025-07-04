using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using System.Reflection;
using UnityEngine;

public class BuildingBarrack : BuildingBase
{
    [Header("Spawn Settings")]
    [Tooltip("Transform indicating where units should appear.")]
    [SerializeField] private Transform _spawnPoint = null;
    [Tooltip("Seconds between each individual unit spawn.")]
    [SerializeField] private float _spawnInterval = 1f;

    [Header("Formation Settings")]
    [Tooltip("Seconds to wait after last spawn before ordering formation.")]
    [SerializeField] private float _formationDelay = 0.5f;

    [Header("Renderers for Selection")]
    [Tooltip("Assign specific mesh renderers to tint on selection.")]
    [SerializeField] private Renderer[] _selectionRenderers;

    private UnitManager _unitManager;
    private ResourceManager _resourceManager;
    private GridManager _gridManager;
    private readonly List<UnitInstance> _spawnedUnits = new List<UnitInstance>();

    public void Initialize(UnitManager unitManager, ResourceManager resourceManager, GridManager gridManager)
    {
        _unitManager = unitManager;
        _resourceManager = resourceManager;
        _gridManager = gridManager;
        Banner.BannerMoved += OnBannerMoved;
    }

    // Public entry point for UI to spawn a single wave of units
    public void SpawnUnit()
    {
        /*
        StartCoroutine(SpawnAndFormWave());
        */
    }

    /*
    private IEnumerator SpawnAndFormWave()
    {
        if (_unitManager == null || _spawnPoint == null || _gridManager == null)
            yield break;

        // 1) Spawn units into a list
        List<UnitInstance> spawned = new List<UnitInstance>();
        yield return StartCoroutine(
            _unitManager.SpawnArmyAndCollect(
                UnitType.Archer,
                team,
                _spawnPoint.position,
                _spawnInterval,
                spawned));

        // 2) Wait before initial formation
        yield return new WaitForSeconds(_formationDelay);

        // 3) Cache spawned units and issue formation
        _spawnedUnits.Clear();
        _spawnedUnits.AddRange(spawned);
        IssueFormationOrders();
    }
    */

    // Computes formation nodes around the banner then issues MoveTo for each spawned unit that is not currently selected
    private void IssueFormationOrders()
    {
        // Always use banner if it exists
        Banner banner = Object.FindAnyObjectByType<Banner>();
        Vector3 centerPos = (banner != null) ? banner.transform.position : _spawnPoint.position;

        GridNode centerNode = _gridManager.GetNodeFromWorldPosition(centerPos);
        List<GridNode> nodes = _gridManager.FindNearestFreeNodes(centerNode, _spawnedUnits.Count);

        for (int i = 0; i < _spawnedUnits.Count; i++)
        {
            UnitInstance unit = _spawnedUnits[i];
            if (unit.IsSelected)
                continue; // skip units currently selected by player

            GridNode target = (i < nodes.Count) ? nodes[i] : centerNode;
            unit.SetReservedDestination(target);
            unit.MoveTo(target);
        }
    }

    private void OnBannerMoved(Vector3 newPosition)
    {
        IssueFormationOrders();
    }

    private void OnDestroy()
    {
        Banner.BannerMoved -= OnBannerMoved;
    }

    public override void OnSelected()
    {
        foreach (Renderer r in _selectionRenderers)
            if (r != null)
                r.material.color = Color.gray;
    }

    public override void OnDeselected()
    {
        ApplyTeamMaterial();
    }

}

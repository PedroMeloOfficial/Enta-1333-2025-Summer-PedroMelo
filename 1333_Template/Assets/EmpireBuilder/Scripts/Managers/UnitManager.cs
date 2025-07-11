using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class UnitManager : MonoBehaviour
{
    // Internal list with all units registered
    private readonly List<UnitBase> allUnits = new List<UnitBase>();

    // Private properties
    [Header("Unit Types for spawn")]
    [SerializeField] private UnitType villager = null;
    [SerializeField] private UnitType regularSword = null;
    [SerializeField] private UnitType regularSpear = null;
    [SerializeField] private UnitType heavySword = null;

    // Public properties
    public IReadOnlyList<UnitBase> AllUnits => allUnits;

    public void RegisterUnit(UnitInstance unit)
    {
        if (unit != null && !allUnits.Contains(unit))
        {
            allUnits.Add(unit);
        }
    }

    public void UnregisterUnit(UnitInstance unit)
    {
        allUnits.Remove(unit);
    }

    private GridManager gridManager;
    private UnitManager unitManager;
    private Pathfinder pathfinder;

    public void Initialize(GridManager _gridManager, UnitManager _unitManager)
    {
        gridManager = _gridManager;
        unitManager = _unitManager;

        if (gridManager == null)
        {
            Debug.LogError("UnitManager: GridManager reference is null");
        }

        if (unitManager == null)
        {
            Debug.LogError("UnitManager: UnitManager reference is null");
        }

        pathfinder = new Pathfinder(gridManager);

        // TESTING UNITS SPAWNING
        if (regularSword != null)
        {
            SpawnArmy(regularSword, Team.Player);
            SpawnArmy(regularSword, Team.Player);
            SpawnArmy(regularSword, Team.Player);
            SpawnArmy(regularSword, Team.Player);
            SpawnArmy(regularSword, Team.Player);

            SpawnArmy(regularSword, Team.Red);
            SpawnArmy(regularSword, Team.Red);
        }
    }

    
    private void Update()
    {
        // Press 1 to spawn Player Spearman army
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (regularSword != null)
            {
                SpawnArmy(regularSword, Team.Player);
            }
        }

        // Press 2 to spawn Enemy Spearman army
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (regularSword != null)
            {
                SpawnArmy(regularSword, Team.Red);
            }
        }

        // Press 3 to spawn Enemy Spearman army
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (heavySword != null)
            {
                SpawnArmy(heavySword, Team.Player);
            }
        }
    }

    private void SpawnArmy(UnitType unit, Team team)
    {
        if (gridManager == null || unitManager == null)
        {
            Debug.LogError("ArmyManager: Must call Initialize(gridManager, unitManager) before spawning.");
            return;
        }

        GameObject prefab = unit.Prefab;

        if (prefab == null)
        {
            Debug.LogWarning($"ArmyManager: Null stats or prefab in ArmyComposition entry for team {team}.");
            return;
        }

        
        // Choose a random spawn position on a walkable node
        GridNode rndWalkableNode = gridManager.GetRandomWalkableNode();
        if (rndWalkableNode == null)
        {
            Debug.LogWarning("ArmyManager: No walkable nodes available to spawn units.");
            return;
        }

        GridNode rndNode = rndWalkableNode;
        Vector3 spawnPosition = rndNode.WorldPosition;
        

        
        // Instantiate the unit prefab at that position
        GameObject unitGO = Instantiate(prefab, spawnPosition, Quaternion.identity); // SOLVE

        // 3) Get the UnitBase component and register it
        UnitInstance unitComponent = unitGO.GetComponent<UnitInstance>();
        if (unitComponent != null)
        {
            // Register the unit with UnitManager
            unitManager.RegisterUnit(unitComponent);

            // 4) Initialize the unit with its type, gridManager, pathfinder, and team
            unitComponent.Initialize(unit, gridManager, pathfinder, team);
        }
        else
        {
            Debug.LogWarning($"ArmyManager: Spawned object {unitGO.name} does not have a UnitBase-derived component.");
            Destroy(unitGO);
        }
        
    }
    
}
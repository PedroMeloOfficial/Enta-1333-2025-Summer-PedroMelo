using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmyComposition", menuName = "Game/Army Composition")]
public class ArmyComposition : ScriptableObject
{
    public string armyName = "New Army";

    [Header("Units to Spawn")]
    public List<UnitEntry> unitEntries = new List<UnitEntry>();
    public UnitType unitType = null;
    public GameObject unitPrefab = null;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingType", menuName = "Game/BuildingType")]
public class BuildingType : ScriptableObject
{
    public List<BuildingData> Buildings = new();
}

[System.Serializable]
public class BuildingData
{
    public string Name;
    public Sprite Icon;
}
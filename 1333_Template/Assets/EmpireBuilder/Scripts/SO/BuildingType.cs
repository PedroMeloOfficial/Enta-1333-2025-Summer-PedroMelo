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
    [Header("General Info")]
    public string Name;
    public Sprite Icon;
    public string Description;

    [Header("Stats")]
    public GameObject BuildingPrefab;
    public GameObject BuildingModel;  
    public int Health;

    [Header("Grid Size")]
    public int SizeX;
    public int SizeZ;
}
using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEngine;

public class BuildingPlacementUI : MonoBehaviour
{
    [SerializeField] private RectTransform LayoutGroupParent;
    [SerializeField] private SelectBuildingButton ButtonPrefab;
    [SerializeField] private BuildingType BuildingData;

    // Start is called before the first frame update
    void Start()
    {
        foreach(BuildingData buildingData in BuildingData.Buildings)
        {
            SelectBuildingButton button = Instantiate(ButtonPrefab, LayoutGroupParent);
            button.Setup(buildingData);
        }
    }

}
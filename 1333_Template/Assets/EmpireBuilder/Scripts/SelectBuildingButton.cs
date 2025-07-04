using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectBuildingButton : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;

    private BuildingData buildingData;
    private BuildingPlacementManager placementManager;

    public void Setup(BuildingData _buildingData, BuildingPlacementManager _placementManager)
    {
        buildingData = _buildingData;
        placementManager = _placementManager;

        buttonText.text = buildingData.Name;
        buttonImage.sprite = buildingData.Icon;

        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnPlaceButtonClicked);
    }

    public void OnPlaceButtonClicked()
    {
        if (buildingData == null || placementManager == null)
        {
            Debug.LogWarning("BuildingButton: Data or PlacementManager is missing.");
            return;
        }

        placementManager.StartPlacement(buildingData);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private MyPlayerInputManager playerInputManager;
    [SerializeField] private Camera cam;

    private void Awake()
    {
        // Checks for null references
        if (gridManager == null || unitManager == null || playerInputManager == null || cam == null)
        {
            Debug.LogError("GameManager: null references found");
            return;
        }

        gridManager.InitializeGrid();
        unitManager.Initialize(gridManager, unitManager);
        playerInputManager.Initialize(cam, gridManager, unitManager);
    }
}
using UnityEngine;

public class MainGameController : MonoBehaviour
{
    [SerializeField] private GridManager gridManger;
    [SerializeField] private SelectAndMoveUnit selectUnitManager;

    public Pathfinding NavGrid { get; private set; }

    private void Awake()
    {
        NavGrid = FindObjectOfType<Pathfinding>();
        if (!gridManger.IsInitialized)
        {
            gridManger.InitializeGrid();
        }

        if (NavGrid != null)
        {
            NavGrid.Initialise(gridManger);
            selectUnitManager.Initialize(gridManger);
        }
    }
}
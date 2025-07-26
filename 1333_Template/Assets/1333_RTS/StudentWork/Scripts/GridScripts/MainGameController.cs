using UnityEngine;

public class MainGameController : MonoBehaviour
{
    [SerializeField] private GridManager manager;

    public Pathfinding NavGrid { get; private set; }

    private void Awake()
    {
        NavGrid = FindObjectOfType<Pathfinding>();
        if (!manager.IsInitialized)
        {
            manager.InitializeGrid();
        }

        if (NavGrid != null)
        {
            NavGrid.Initialise(manager);
        }
    }
}
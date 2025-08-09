using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlacementHandler : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private GridManager grid;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float liftOffset = 0;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] prefabs; // 0: Friendly Unit, 1: Archer Tower
    [SerializeField] private Vector2Int[] sizes = {
        new Vector2Int(1, 1), // Friendly Unit
        new Vector2Int(1, 1)  // Archer Tower
    };

    [Header("Materials")]
    [SerializeField] private Material okMat;
    [SerializeField] private Material badMat;

    [Header("Currency Settings")]
    [SerializeField] private int startingCurrency = 0;
    [SerializeField] private int incomePerSecond = 1;
    [SerializeField] private int[] placementCosts = { 2, 20 }; // Friendly, ArcherTower

    private int currency;
    private float incomeTimer;

    GameObject ghost;
    int current = -1;
    bool valid;
    int originX, originY;
    Vector3 snapPos;

    private void Start()
    {
        currency = startingCurrency;
        UpdateCurrencyUI();
    }

    private void Update()
    {
        HandleCurrencyTick();
        Hotkeys();
        if (ghost == null) return;
        Snap();
        if (Mouse.current.leftButton.wasPressedThisFrame && valid)
            Place();
    }

    private void HandleCurrencyTick()
    {
        incomeTimer += Time.deltaTime;
        if (incomeTimer >= 1f)
        {
            currency += incomePerSecond;
            UpdateCurrencyUI();
            incomeTimer = 0f;
        }
    }

    private void Hotkeys()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) Set(0); // Friendly Unit
        if (Keyboard.current.digit2Key.wasPressedThisFrame) Set(1); // Archer Tower
    }

    private void Set(int index)
    {
        if (index == current || index < 0 || index >= prefabs.Length) return;
        if (currency < placementCosts[index]) return;

        current = index;
        
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buildingSelectClip);
        
        if (ghost != null) Destroy(ghost);
        ghost = Instantiate(prefabs[index]);
        ghost.tag = "Untagged";
        ghost.layer = 0;
        SetMat(okMat);
    }

    private void Snap()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out var hit, 1000f, groundMask)) return;

        GridNode node = grid.GetNodeFromWorldPosition(hit.point);
        if (node == null) return;

        GridSettings settings = grid.GridSettings;
        Vector2Int sz = sizes[current];

        Vector3 local = grid.transform.InverseTransformPoint(node.WorldPosition);
        int gx = Mathf.RoundToInt(local.x / settings.NodeSize);
        int gy = Mathf.RoundToInt(local.z / settings.NodeSize);

        originX = Mathf.Clamp(gx, 0, settings.GridSizeX - sz.x);
        originY = Mathf.Clamp(gy, 0, settings.GridSizeY - sz.y);

        snapPos = grid.GetNodeAt(originX, originY).WorldPosition;

        float lift = settings.NodeSize * 0.5f;
        ghost.transform.position = snapPos + Vector3.up * lift;

        valid = CheckRegion(originX, originY, sz.x, sz.y);
        SetMat(valid ? okMat : badMat);
    }

    private bool CheckRegion(int ox, int oy, int w, int h)
    {
        for (int dx = 0; dx < w; dx++)
        for (int dy = 0; dy < h; dy++)
            if (!grid.GetNodeAt(ox + dx, oy + dy).Walkable) return false;

        return true;
    }

    private void Place()
    {
        if (currency < placementCosts[current]) return;

        float lift = grid.GridSettings.NodeSize * 0.5f;
        GameObject obj = Instantiate(prefabs[current], snapPos + Vector3.up * lift, Quaternion.identity);
        obj.tag = prefabs[current].tag;
        obj.layer = prefabs[current].layer;

        Pathfinding finder = FindObjectOfType<Pathfinding>();
        if (obj.TryGetComponent<UnitAI>(out var ai)) ai.Initialise(grid, finder);
        if (obj.TryGetComponent<UnitMover>(out var mover)) mover.Inject(grid, finder);
        if (obj.TryGetComponent<ArcherTower>(out var turret)) turret.Initialize(grid);

        Vector2Int sz = sizes[current];
        for (int dx = 0; dx < sz.x; dx++)
        for (int dy = 0; dy < sz.y; dy++)
        {
            GridNode node = grid.GetNodeAt(originX + dx, originY + dy);
            if (node == null) continue;
            if (current == 1) node.Walkable = false; // Only Archer Tower blocks walkability
            node.Occupant = obj;
        }

        currency -= placementCosts[current];
        UpdateCurrencyUI();
        
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buildingPlaceClip);

        Destroy(ghost);
        ghost = null;
        current = -1;
    }

    private void UpdateCurrencyUI()
    {
        if (currencyText != null)
            currencyText.text = currency.ToString();
    }

    private void SetMat(Material mat)
    {
        foreach (var r in ghost.GetComponentsInChildren<Renderer>())
            r.material = mat;
    }
}

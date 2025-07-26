using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementHandler : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private GridManager grid;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float liftOffset = 0;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Vector2Int[] sizes = {
        new Vector2Int(2,2),
        new Vector2Int(1,1),
        new Vector2Int(1,1),
        new Vector2Int(1,1)
    };

    [Header("Materials")]
    [SerializeField] private Material okMat;
    [SerializeField] private Material badMat;

    GameObject ghost;
    int current = -1;
    bool valid;
    int originX, originY;
    Vector3 snapPos;

    private void Update()
    {
        Hotkeys();
        if (ghost == null) return;
        Snap();
        if (Mouse.current.leftButton.wasPressedThisFrame && valid)
            Place();
    }

    private void Hotkeys()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) Set(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) Set(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) Set(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) Set(3);
    }

    private void Set(int index)
    {
        if (index == current || index < 0 || index >= prefabs.Length) return;
        current = index;
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

        float lift = (current == 2 || current == 3) ? 0f : settings.NodeSize * 0.5f;
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
        bool building = (current == 2 || current == 3);
        float lift = building ? grid.GridSettings.NodeSize * 0.5f : 0f;

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
            if (building) node.Walkable = false;
            node.Occupant = obj;
        }

        Destroy(ghost);
        ghost = null;
        current = -1;
    }

    private void SetMat(Material mat)
    {
        foreach (var r in ghost.GetComponentsInChildren<Renderer>())
            r.material = mat;
    }
}
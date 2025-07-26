using UnityEngine;

[RequireComponent(typeof(HPStat))]
public class ArcherTower : MonoBehaviour
{
    [SerializeField] private GridManager grid;
    [SerializeField] private int rangeTiles = 4;
    [SerializeField] private float fireRate = 3f;
    [SerializeField] private int bulletDamage = 5;
    [SerializeField] private Transform barrel;
    [SerializeField] private float interval = 0.3f;

    private float scanTick;
    private float shotTick;
    private HPStat currentTarget;

    public void Initialize(GridManager gm) => grid = gm;

    private void Update()
    {
        if (grid == null) return;

        if (!IsValid(currentTarget))
            currentTarget = null;

        scanTick -= Time.deltaTime;
        if (scanTick <= 0f)
        {
            AcquireTarget();
            scanTick = interval;
        }

        if (currentTarget != null)
        {
            shotTick -= Time.deltaTime;
            if (shotTick <= 0f)
            {
                Fire();
                shotTick = 1f / fireRate;
            }
        }
    }

    private void AcquireTarget()
    {
        currentTarget = null;

        Collider[] results = Physics.OverlapBox(transform.position,
            Vector3.one * rangeTiles * grid.GridSettings.NodeSize * 0.5f,
            Quaternion.identity,
            LayerMask.GetMask("Enemy"));

        float best = float.MaxValue;

        foreach (var c in results)
        {
            var hp = c.GetComponent<HPStat>();
            if (!IsValid(hp)) continue;

            float d = (hp.transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                currentTarget = hp;
            }
        }
    }

    private bool IsValid(HPStat hp)
    {
        if (hp == null || hp.Current <= 0) return false;

        float size = grid.GridSettings.NodeSize;
        float dist = rangeTiles * size;
        if ((hp.transform.position - transform.position).sqrMagnitude > dist * dist) return false;

        return true;
    }

    private void Fire()
    {
        if (currentTarget == null || currentTarget.Current <= 0)
        {
            currentTarget = null;
            return;
        }

        currentTarget.TakeDamage(bulletDamage, gameObject);
        Debug.DrawLine(barrel ? barrel.position : transform.position + Vector3.up,
            currentTarget.transform.position + Vector3.up,
            Color.red, 0.1f);
    }
}

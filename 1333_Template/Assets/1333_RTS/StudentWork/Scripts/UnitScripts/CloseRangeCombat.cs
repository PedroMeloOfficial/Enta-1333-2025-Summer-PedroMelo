using UnityEngine;

[RequireComponent(typeof(UnitAI))]
public class CloseRangeCombat : MonoBehaviour
{
    [SerializeField] private float hitsPerSecond = 1f;
    [SerializeField] private int damagePerHit = 10;
    [SerializeField] private float range = 1.2f;

    private UnitAI brain;
    private float cooldown;

    private void Awake()
    {
        brain = GetComponent<UnitAI>();
    }

    private void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        GameObject targetObj = brain.CurrentTarget;
        if (targetObj == null) return;

        if (Vector3.Distance(transform.position, targetObj.transform.position) <= range)
        {
            if (cooldown <= 0f)
            {
                if (targetObj.TryGetComponent<IDamageReceiver>(out var hp))
                {
                    hp.TakeDamage(damagePerHit, gameObject);
                }
                cooldown = 1f / hitsPerSecond;
            }
        }
    }
}
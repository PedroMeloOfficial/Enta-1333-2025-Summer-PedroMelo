using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float velocity = 15f;
    private GameObject aimTarget;
    private int damageToDeal;

    public void Launch(GameObject destination, int amount)
    {
        aimTarget = destination;
        damageToDeal = amount;
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (aimTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, aimTarget.transform.position, velocity * Time.deltaTime);

        float closeEnough = 0.2f;
        if (Vector3.Distance(transform.position, aimTarget.transform.position) < closeEnough)
        {
            if (aimTarget.TryGetComponent<IDamageReceiver>(out var targetHP))
            {
                targetHP.TakeDamage(damageToDeal, gameObject);
            }
            Destroy(gameObject);
        }
    }
}
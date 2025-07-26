using UnityEngine;

public interface IDamageReceiver
{
    void TakeDamage(int amount, GameObject from);
}
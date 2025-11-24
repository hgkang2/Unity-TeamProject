using UnityEngine;

public class FixedTrap : MonoBehaviour
{
    public float damage = 8;

    void OnCollisionEnter2D(Collision2D other)
    {
        IDamageable target = other.gameObject.GetComponent<IDamageable>();
        if (target == null) return;
        target.TakeDamage(damage);
    }
}

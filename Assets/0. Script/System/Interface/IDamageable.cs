using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition = null);
}

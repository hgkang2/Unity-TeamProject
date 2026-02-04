using UnityEngine;

public class GrenadeHitbox : MonoBehaviour
{
    [SerializeField] float damage = 10f;
    [SerializeField] DamageType damageType;
    

    void Awake()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 hitPos = transform.position;
            damageable.TakeDamage(damage, damageType, hitPos);
        }
    }
}

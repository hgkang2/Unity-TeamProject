using UnityEngine;

public class OnContactDamage : MonoBehaviour
{
    [SerializeField] float collideDamage;
    [SerializeField] DamageType damageType;
    [SerializeField] Collider2D col;

    private void Awake()
    {
        if(!col) col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 hitPos = transform.position;
            damageable.TakeDamage(collideDamage, damageType, hitPos);
        }
    }
}

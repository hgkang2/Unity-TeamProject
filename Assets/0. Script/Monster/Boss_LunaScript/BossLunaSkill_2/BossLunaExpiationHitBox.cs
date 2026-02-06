using UnityEngine;

public class BossLunaExpiationHitBox : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] DamageType damageType;
    [SerializeField] Collider2D col;
    
    BossLuna bossLuna;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // if(collision.gameObject.CompareTag("Player"))
        // {
        //     bossLuna.hasSkillBHit = true; 
        // }
        if(!collision.gameObject.CompareTag("Player")) return;

        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 hitPos = transform.position;
            damageable.TakeDamage(damage, DamageType.Normal, hitPos);
            bossLuna.hasSkillBHit = true;
        }
    }
}

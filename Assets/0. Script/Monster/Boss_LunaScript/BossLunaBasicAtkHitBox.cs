using System.ComponentModel;
using UnityEngine;

public class BossLunaBasicAtkHitBox : MonoBehaviour
{
    [SerializeField] float damage = 10f;
    [SerializeField] DamageType damageType;
    [SerializeField] Collider2D col;
    //LocalSFX sfx;

    BossLuna bossLuna;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player")) return;

        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 hitPos = transform.position;
            damageable.TakeDamage(damage,damageType, hitPos);

            bossLuna.attackHitCombo++;
        }
    }
}

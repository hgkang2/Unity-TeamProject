using UnityEngine;
using System;

public class SkillHitBox : MonoBehaviour
{
    [SerializeField] float damage = 10f;
    [SerializeField] DamageType damageType;

    [SerializeField] Collider2D col;

    //public event Action<IDamageable> OnHit;
    LocalSFX sfx;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sfx = GetComponent<LocalSFX>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 hitPos = transform.position;
            damageable.TakeDamage(damage, damageType, hitPos);
            //OnHit?.Invoke(damageable);
            //sfx.Play("DamageSound");
            //Debug.Log("Damage");
        }
    }
}

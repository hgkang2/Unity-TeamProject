using UnityEngine;
using System;

public class SkillHitBox : MonoBehaviour
{
    public float damage = 10f;
    public DamageType damageType;

    public Collider2D col;

    public event Action<IDamageable> OnHit;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 hitPos = transform.position;
            damageable.TakeDamage(damage, damageType, hitPos);
            //OnHit?.Invoke(damageable);
            //Debug.Log("Damage");
        }
    }
}

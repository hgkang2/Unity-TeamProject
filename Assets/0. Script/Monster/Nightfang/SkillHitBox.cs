using UnityEngine;
using System;

public class SkillHitBox : MonoBehaviour
{
    [SerializeField] float damage = 10f;
    [SerializeField] DamageType damageType;

    Collider2D col;

    LocalSFX sfx;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sfx = GetComponent<LocalSFX>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<Player>(out var player))
        {
            Vector2 hitPos = transform.position;
            player.TakeDamage(damage, damageType, hitPos);
            //OnHit?.Invoke(damageable);
            //sfx.Play("DamageSound");
            //Debug.Log("Damage");
        }
    }
}

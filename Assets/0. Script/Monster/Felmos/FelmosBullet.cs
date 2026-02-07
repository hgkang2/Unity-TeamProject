using UnityEngine;
using System;

public class FelmosBullet : MonoBehaviour
{
    Collider2D cd;
    Rigidbody2D rb;

    [SerializeField] DamageType damageType;
    [SerializeField] float damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<Collider2D>();
    }

    public void Initialize(Vector2 direction, float dmg)
    {
        this.damage = dmg;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * 20f, ForceMode2D.Impulse);

        Destroy(this.gameObject, 5f);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out var player))
        {
            Vector2 hitPos = transform.position;
            player.TakeDamage(damage, damageType, hitPos);
            //OnHit?.Invoke(damageable);
            //sfx.Play("DamageSound");
            //Debug.Log("Damage");
            Destroy(this.gameObject);
        }
    }
}

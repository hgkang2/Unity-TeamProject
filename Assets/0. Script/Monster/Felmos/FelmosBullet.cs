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
        if(collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground"))
        {
            Destroy(this.gameObject);
        }

        if(!collision.CompareTag("Player")) return;

        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 hitPos = transform.position;
            damageable.TakeDamage(damage, damageType, hitPos);

            Destroy(this.gameObject,0.1f);
        }
    }
}

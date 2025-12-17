using UnityEngine;
using System;

public class SkillHitBox : MonoBehaviour
{
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
            OnHit?.Invoke(damageable);
            Debug.Log("Damage");
        }
    }
}

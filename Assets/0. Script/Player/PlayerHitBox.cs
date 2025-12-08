using UnityEngine;
using System;

public class PlayerHitBox : MonoBehaviour
{
    public Collider2D col;

    public event Action<IDamageable> OnHit;

    void Awake(){
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            OnHit?.Invoke(damageable);
        }
    }
}

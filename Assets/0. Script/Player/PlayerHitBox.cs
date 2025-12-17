using UnityEngine;
using System;

public class PlayerHitBox : MonoBehaviour
{
    public Collider2D col;

    public event Action<Collider2D> OnHit;

    void Awake(){
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        OnHit?.Invoke(other);
    }
}

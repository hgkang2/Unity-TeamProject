using UnityEngine;
using System;

public class BossLunaHolyGrenade : MonoBehaviour
{
    public GameObject ExplosionEffect;
    public float throwPower;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.AddForce(Vector2.left * throwPower, ForceMode2D.Impulse);
    }   

    void OnCollisionEnter2D(Collision2D collision)
    {

    }
}

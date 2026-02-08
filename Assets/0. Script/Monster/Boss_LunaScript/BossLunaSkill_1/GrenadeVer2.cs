using UnityEngine;
using System;
using System.Collections;

public class GrenadeVer2 : MonoBehaviour
{
    public Transform warningSignPos;
    public GameObject ExplosionEffect;
    SpriteRenderer sr;
    GrenadeTrajectory traj;
    Vector2 targetPos;
    GameObject owner;
    Rigidbody2D rb;
    LocalSFX localSFX;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if(warningSignPos) warningSignPos.gameObject.SetActive(false);
        traj = GetComponent<GrenadeTrajectory>();
        localSFX = GetComponent<LocalSFX>();
    }


    public void InitializeGrenadeThrow(Vector2 targetPos, float travelTime, GameObject owner)
    {
        this.targetPos = targetPos;
        this.owner = owner;
        travelTime  = 0.5f;
        // if(warningSignPos)
        // {
        //     warningSignPos.SetParent(null);
        //     warningSignPos.position = targetPos + Vector2.up * 1f;
        //     warningSignPos.gameObject.SetActive(true);
        // }

        traj?.Show(targetPos, travelTime);
        //ThrowGrenade(targetPos, travelTime);

        ThrowStraight(targetPos, travelTime);
    }

    public void ThrowStraight(Vector2 targetPos, float travelTime)
    {
        Vector2 start = transform.position;
        Vector2 dir = (targetPos - start);
        float distance = dir.magnitude;

        rb.gravityScale = 0f;
        rb.linearVelocity = dir.normalized * (distance / travelTime);
    }

    bool isExploding = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExploding) return;
        if (collision.CompareTag("Wall") || collision.CompareTag("Ground") || collision.CompareTag("Player"))
        {
            isExploding = true;
            ExplodeRoutine();
        }
    }

    void ExplodeRoutine()
    {
        warningSignPos.gameObject.SetActive(false);
        traj?.Hide();
        localSFX.Play("Explode");
        Color c = sr.color;
        c.a = 0f;
        Instantiate(ExplosionEffect, transform.position, Quaternion.identity);

        Destroy(this.gameObject);
    }
}

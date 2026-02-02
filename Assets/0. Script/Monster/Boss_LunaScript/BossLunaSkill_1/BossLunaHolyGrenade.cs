using UnityEngine;
using System;
using System.Collections;

public class BossLunaHolyGrenade : MonoBehaviour
{
    public Transform warningSignPos;
    public GameObject ExplosionEffect;
    GrenadeTrajectory traj;
    Vector2 targetPos;
    GameObject owner;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if(warningSignPos) warningSignPos.gameObject.SetActive(false);
        traj = GetComponent<GrenadeTrajectory>();
    }
    
    bool hasTarget;
    bool exploded;
    public float explodeRadius;
    void FixedUpdate()
    {   
        if (!hasTarget || exploded) return;

        float sqrDist = ((Vector2)transform.position - targetPos).sqrMagnitude;
        if (sqrDist <= explodeRadius)
        {
            exploded = true;
            StartCoroutine(ExplodeRoutine());
        }
    }

    public void InitializeGrenadeThrow(Vector2 targetPos, float travelTime, GameObject owner)
    {
        this.targetPos = targetPos;
        this.owner = owner;
        hasTarget = true;
        exploded = false;

        if(warningSignPos)
        {
            warningSignPos.SetParent(null);
            warningSignPos.position = targetPos + Vector2.up * 1f;
            warningSignPos.gameObject.SetActive(true);
        }

        traj?.Show(targetPos, travelTime);
        ThrowGrenade(targetPos, travelTime);
    }

    void ThrowGrenade(Vector2 targetPos, float travelTime)
    {
        Vector2 start = rb.position;
        Vector2 velocity = CaculateVelocity(start, targetPos, travelTime);

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = velocity;
    }

    public float yVel;
    Vector2 CaculateVelocity(Vector2 start, Vector2 target, float time)
    {
        Vector2 distance = target - start;
        float gravity = -Physics2D.gravity.y * rb.gravityScale;

        float velocityX = distance.x / time;
        float velocityY = (distance.y + yVel * gravity * time * time) / time;

        return new Vector2(velocityX, velocityY);
    }

    IEnumerator ExplodeRoutine()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        Destroy(warningSignPos.gameObject);
        traj?.Hide();
        yield return new WaitForSeconds(0.5f);

        Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
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

    public void InitializeGrenadeThrow(Vector2 targetPos, float travelTime, GameObject owner)
    {
        this.targetPos = targetPos;
        this.owner = owner;

        if(warningSignPos)
        {
            warningSignPos.SetParent(null);
            warningSignPos.position = targetPos + Vector2.up * 1f;
            warningSignPos.gameObject.SetActive(true);
        }

        //traj?.Show(targetPos, travelTime);
        ThrowGrenade(targetPos, travelTime);
    }

    void ThrowGrenade(Vector2 targetPos, float travelTime)
    {
        Vector2 start = rb.position;
        Vector2 velocity = CaculateVelocity(start, targetPos, travelTime);

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = velocity;
    }

    Vector2 CaculateVelocity(Vector2 start, Vector2 target, float time)
    {
        Vector2 distance = target - start;
        float gravity = -Physics2D.gravity.y * rb.gravityScale;

        float velocityX = distance.x / time;
        float velocityY = (distance.y + 0.5f * gravity * time * time) / time;

        return new Vector2(velocityX, velocityY);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Player"))
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            Destroy(warningSignPos.gameObject);
            traj?.Hide();

            StartCoroutine(ExplodeRoutine());
        }
    }

    IEnumerator ExplodeRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
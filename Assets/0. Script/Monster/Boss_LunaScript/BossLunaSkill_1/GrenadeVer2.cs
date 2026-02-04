using UnityEngine;
using System;
using System.Collections;

public class GrenadeVer2 : MonoBehaviour
{
    public Transform warningSignPos;
    public GameObject ExplosionEffect;
    [SerializeField] GameObject hitbox;
    SpriteRenderer sr;
    GrenadeTrajectory traj;
    Vector2 targetPos;
    GameObject owner;
    Rigidbody2D rb;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if(warningSignPos) warningSignPos.gameObject.SetActive(false);
        traj = GetComponent<GrenadeTrajectory>();
    }


    public void InitializeGrenadeThrow(Vector2 targetPos, float travelTime, GameObject owner)
    {
        this.targetPos = targetPos;
        this.owner = owner;
        travelTime  = 0.3f;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(ExplodeRoutine());
        }
    }

    IEnumerator ExplodeRoutine()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        Destroy(warningSignPos.gameObject);
        traj?.Hide();
        yield return new WaitForSeconds(0.3f);

        Color c = sr.color;
        c.a = 0f;
        Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
        
        hitbox.SetActive(true);

        yield return new WaitForSeconds(0.3f);
        Destroy(this.gameObject);
    }
}

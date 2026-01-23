using UnityEngine;
using System;
using UnityEditor.Callbacks;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class BossLuna : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Detect")]
    public float aggroRange;
    public float distanceToPlayer;
    public float distanceOfX;
    public LayerMask playerMask;

    [Header("SkillA")]
    public GameObject holyGnadePrefab;
    public float JumpYForce;
    public float JumpXForce;
    public bool canUseSkillA = true;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        PlayerDetect();

        TempSkill_A(); 
    }

    void PlayerDetect()
    {
        if (!playerTransform)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, aggroRange, playerMask);

            if(hit)
                playerTransform = hit.transform;
            else
            {
                distanceToPlayer = float.PositiveInfinity;
                distanceOfX = 0f;
                return;
            }
        }

        if(!playerTransform.gameObject.activeInHierarchy)
        {
            playerTransform = null;
            distanceToPlayer = float.PositiveInfinity;
            distanceOfX = 0f;
            return;
        }

        Vector2 myPos = transform.position;
        Vector2 playerPos = playerTransform.position;

        Vector2 toPlayer = playerPos - myPos;
        distanceOfX = toPlayer.x;
        distanceToPlayer = toPlayer.magnitude;
    }

    void TempSkill_A()
    {
        if(distanceToPlayer <= aggroRange && canUseSkillA)
        {
            canUseSkillA = false;
            StartCoroutine(SkillARoutine());
        }
    }

    IEnumerator SkillARoutine()
    {
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.6f);

        rb.AddForce(new Vector2(JumpXForce, JumpYForce), ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.4f);
         
        ThrowGrenade();
    }

    void ThrowGrenade()
    {
        Instantiate(holyGnadePrefab, transform.position, Quaternion.identity);
        return;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}

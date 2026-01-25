using UnityEngine;
using System;
using UnityEditor.Callbacks;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;

public class BossLuna : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Detect")]
    public float aggroRange;
    public float distanceToPlayer;
    public float distanceOfX;
    public LayerMask playerMask;

    [Header("Attack")]
    

    [Header("SkillA")]
    public bool canUseSkillA = true;
    public GameObject holyGrenadePrefab;
    public Transform throwPos;
    public float JumpYForce;
    public float JumpXForce;
    public float sideOffset;
    public float grenadeTravelTime;
    Vector2 targetPos;

    [Header("SkillB")]
    public bool canUseSkillB;

    [Header("Player Pos Check")]
    public LayerMask groundMask;
    public float groundRayLength;
    Vector2 cachedTargetPos;
    bool hasCachedTarget;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        PlayerDetect();

        Skill_A(); 
    }

    void PlayerDetect()
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

        if(!playerTransform.gameObject.activeInHierarchy)
        {
            playerTransform = null;
            distanceToPlayer = float.PositiveInfinity;
            distanceOfX = 0f;
            return;
        }

        Vector2 myPos = transform.position;
        Vector2 playerPos = playerTransform.position;
        targetPos = playerTransform.position;

        Vector2 toPlayer = playerPos - myPos;
        distanceOfX = toPlayer.x;
        distanceToPlayer = toPlayer.magnitude;
    }

    void Skill_A()
    {
        if(canUseSkillA)
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
        CachPlayerPos();
        
        ThrowGrenadeEvent();
    }

    void ThrowGrenadeEvent()
    {
        if(!hasCachedTarget) return;

        Vector2 center = cachedTargetPos;

        Vector2 left = center + Vector2.left * sideOffset;
        Vector2 right = center + Vector2.right * sideOffset;

        ThrowGrenade(left);
        ThrowGrenade(center);
        ThrowGrenade(right);

        hasCachedTarget = false;
    }

    void ThrowGrenade(Vector2 targetPos)
    {
        var grenade = Instantiate(holyGrenadePrefab, throwPos.position, Quaternion.identity);
        grenade.GetComponent<BossLunaHolyGrenade>().InitializeGrenadeThrow(targetPos, grenadeTravelTime, gameObject);

        hasCachedTarget = false;
    }

    void CachPlayerPos()
    {
        if(!playerTransform) { hasCachedTarget = false; return;}

        Vector2 origin = (Vector2)playerTransform.position + Vector2.up * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundMask);

        Debug.DrawRay(origin, Vector2.down * groundRayLength, Color.yellow, 0.2f);

        if(hit.collider != null)
        {
            cachedTargetPos = hit.point;
            hasCachedTarget = true;
        }
        else
        {
            cachedTargetPos = playerTransform.position;
            hasCachedTarget = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}

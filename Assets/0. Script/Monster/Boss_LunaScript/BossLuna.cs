using UnityEngine;
using System;
using UnityEditor.Callbacks;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using Unity.Mathematics;

public class BossLuna : MonoBehaviour
{
    #region Variables
    public Transform playerTransform;

    [Header("Detect")]
    public float aggroRange;
    public float distanceToPlayer;
    public float distanceOfX;
    public LayerMask playerMask;

    [Header("Attack")]
    

    [Header("SkillA")]
    public bool canUseSkillA;
    public GameObject holyGrenadePrefab;
    public Transform throwPos;
    public float JumpYForce;
    public float JumpXForce;
    public float sideOffset;
    public float grenadeTravelTime;
    Vector2 targetPos;

    [Header("SkillB")]
    public bool canUseSkillB;
    public GameObject expiationPrefab;
    public float duration;

    [Header("SkillC")]
    public bool canUseSkillC;
    public GameObject genesisPrefab;

    [Header("Player Pos Check")]
    public LayerMask groundMask;
    public float groundRayLength;
    Vector2 cachedTargetPos;
    bool hasCachedTarget;

    Rigidbody2D rb;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        PlayerDetect();

        Skill_A(); 
        Skill_B();
        Skill_CTrackVer();
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

    #region Skill_A
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
    #endregion

    #region  Skill_B
    void Skill_B()
    {
        if(canUseSkillB)
        {
            canUseSkillB = false;
            StartCoroutine(SkillBRoutine());
        }
    }

    IEnumerator SkillBRoutine()
    {
        yield return new WaitForSeconds(0.7f);

        CachPlayerPos();

        ExpiationEvent();
    }

    void ExpiationEvent()
    {
        if(!hasCachedTarget) return;
        Vector2 expiationSpawnpoint = cachedTargetPos + Vector2.up * 0.5f;

        var expiation = Instantiate(expiationPrefab, expiationSpawnpoint, expiationPrefab.transform.rotation);
        expiation.GetComponent<BossLunaExpiation>().InitializeExpiation(expiationSpawnpoint, duration, gameObject);

        hasCachedTarget = false;
    }
    #endregion

    #region Skill_C(TrackVer)
    void Skill_CTrackVer()
    {
        if(canUseSkillC)
        {
            canUseSkillC = false;
            CachPlayerPos();
            GenesisEvent();
        }
    }

    void GenesisEvent()
    {
        if(!hasCachedTarget) return;
        Vector2 genesisSpawnpoint = cachedTargetPos + Vector2.up * 7f;

        var genesis = Instantiate(genesisPrefab, genesisSpawnpoint, quaternion.identity);
        genesis.GetComponent<BossLunaGenesis>().InitializeGenesis(genesisSpawnpoint, 1f, gameObject);

        hasCachedTarget = false;
    }
    #endregion

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

    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, aggroRange);
    // }
}

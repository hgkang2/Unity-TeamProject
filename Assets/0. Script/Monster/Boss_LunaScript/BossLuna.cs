using UnityEngine;
using System;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using Unity.Mathematics;
using JetBrains.Annotations;

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
    public float skillADelay;
    public float skillAThrowMoment;
    public float JumpYForce;
    public float JumpXForce;
    public float sideOffset; // 투척한 3개의 수류탄 사이의 간격
    public float grenadeTravelTime;
    Vector2 targetPos;

    [Header("SkillB")]
    public bool canUseSkillB;
    public GameObject expiationPrefab;
    public float skillBDelay;
    public float skillBSpawnTime;

    [Header("SkillC_Track")]
    public bool canUseSkillC_Trackver;
    public GameObject genesisPrefab;
    public float skillCSpawnTime;
    public float skillCTrackDuration = 5f;
    public float skillCTrackInterval = 1.2f;
    public float skillCTrackElapsed = 0f;

    [Header("SkillC_Random")]
    public bool canUseSkillC_RandomVer;
    public Collider2D bossRoomArea;
    public float skillCRandDuration = 4f;
    public float skillCRandInterval = 0.8f;
    public float skillCminDistance = 2f; 
    public int maxTries = 20;
    public float skillCSpawnY = 6.5f;
    public int skillCRandomPosMemoryCount = 6;

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
        Skill_CRandVer();
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

        yield return new WaitForSeconds(skillADelay);

        rb.AddForce(new Vector2(JumpXForce, JumpYForce), ForceMode2D.Impulse);

        yield return new WaitForSeconds(skillAThrowMoment);
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

    #region Skill_B
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
        yield return new WaitForSeconds(skillBDelay);

        CachPlayerPos();

        ExpiationEvent();
    }

    void ExpiationEvent()
    {
        if(!hasCachedTarget) return;
        Vector2 expiationSpawnpoint = cachedTargetPos + Vector2.up * 0.5f;

        var expiation = Instantiate(expiationPrefab, expiationSpawnpoint, expiationPrefab.transform.rotation);
        expiation.GetComponent<BossLunaExpiation>().InitializeExpiation(expiationSpawnpoint, skillBSpawnTime, gameObject);

        hasCachedTarget = false;
    }
    #endregion

    #region Skill_C(TrackVer)
    void Skill_CTrackVer()
    {
        if(canUseSkillC_Trackver)
        {
            canUseSkillC_Trackver = false;
            StartCoroutine(SkillCTrackRoutine());
        }
    }

    void GenesisEvent()
    {
        if(!hasCachedTarget) return;
        Vector2 genesisSpawnpoint = cachedTargetPos + Vector2.up * genesisPrefab.transform.position.y;

        var genesis = Instantiate(genesisPrefab, genesisSpawnpoint, genesisPrefab.transform.rotation);
        genesis.GetComponent<BossLunaGenesis>().InitializeGenesis(genesisSpawnpoint, skillCSpawnTime, gameObject);

        hasCachedTarget = false;
    }

    IEnumerator SkillCTrackRoutine()
    {
        while (skillCTrackElapsed < skillCTrackDuration)
        {
            CachPlayerPos();

            GenesisEvent();

            yield return new WaitForSeconds(skillCTrackInterval);

            skillCTrackElapsed += skillCTrackInterval;
        }
    }
    #endregion

    #region Skill_C(RandomVer)
    void Skill_CRandVer()
    {
        if(canUseSkillC_RandomVer)
        {
            canUseSkillC_RandomVer = false;
            StartCoroutine(SkillCRandomRoutine());
        }
    }

    IEnumerator SkillCRandomRoutine()
    {
        float endTime = Time.time + skillCRandDuration;

        Bounds b = bossRoomArea.bounds;

        Queue<float> recentXs = new Queue<float>();

        while (Time.time < endTime)
        {
            float x = GetRandomXWithMinDistanceAll(b, recentXs, skillCminDistance, maxTries);

            Vector2 pos = new Vector2(x, skillCSpawnY);

            cachedTargetPos = pos;
            hasCachedTarget = true;
            GenesisEvent();

            // 기록 업데이트
            recentXs.Enqueue(x); // 아래 코드에서 랜덤으로 뽑은 좌표 저장
            if (recentXs.Count > skillCRandomPosMemoryCount) recentXs.Dequeue(); // 가장 오래된 좌표값 제거

            yield return new WaitForSeconds(skillCRandInterval);
        }
    }

    float GetRandomXWithMinDistanceAll(Bounds b, IEnumerable<float> usedXs, float minDist, int tries)
    {
        for (int i = 0; i < tries; i++)
        {
            float x = UnityEngine.Random.Range(b.min.x, b.max.x); // 범위 내의 랜덤 x 좌표 tries 만큼 뽑고

            // 이전 사용한 좌표들과 가까운지 확인
            bool ok = true;
            foreach (float ux in usedXs)
            {
                if (Mathf.Abs(x - ux) < minDist)
                {
                    ok = false; // 조건에 부합하지 않는다면 버리고 다음걸로
                    break;
                }
            }

            if (ok) return x; // 충족되면 해당 좌표 return
        }

        return UnityEngine.Random.Range(b.min.x, b.max.x); // 무한 루프 방지 코드(좌표가 없으면 씹고 밀어내서 생성 위치 지정)
    }

    // Vector2 GetRandomPointWithMinDistance(Bounds b, Vector2? lastPos, float minDist, int tries)
    // {
    //     for (int i = 0; i < tries; i++)
    //     {
    //         float x = UnityEngine.Random.Range(b.min.x, b.max.x);
    //         Vector2 p = new Vector2(x, skillCSpawnY);

    //         if (!lastPos.HasValue) return p;

    //         if (Vector2.Distance(p, lastPos.Value) >= minDist)
    //             return p;
    //     }

    //     return new Vector2(UnityEngine.Random.Range(b.min.x, b.max.x), skillCSpawnY);
    // }
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

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

    [Header("Skill Test")]
    public bool canUseSkillA;
    public bool canUseSkillA_Ver2;
    public bool canUseSkillB;
    public bool canUseSkillC_Trackver;
    public bool canUseSkillC_RandomVer;

    [Header("Detect")]
    public float aggroRange;
    public float distanceToPlayer;
    public float distanceOfX;
    public LayerMask playerMask;

    [Header("Attack")]
    

    [Header("SkillA")]
    public GameObject holyGrenadePrefab;
    public Transform throwPos;
    public float skillADelay;
    public float skillAThrowMoment;
    public float JumpYForce;
    public float JumpXForce;
    public float rollSpeed;
    public float sideOffset; // 투척한 3개의 수류탄 사이의 간격
    public float grenadeTravelTime;
    Vector2 targetPos;

    [Header("SkillA Ver2")]
    public GameObject grenadeVer2;
    public float jumpTimeFar;
    public float jumpTimeNear;

    [Header("SkillB")]
    public GameObject expiationPrefab;
    public float skillBDelay;
    public float skillBSpawnTime;

    [Header("SkillC_Track")]
    public GameObject genesisPrefab;
    public float skillCSpawnTime;
    public float skillCTrackDuration = 5f;
    public float skillCTrackInterval = 1.2f;
    public float skillCTrackElapsed = 0f;

    [Header("SkillC_Random")]
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

    int facingX = 1;
    Vector3 originScale;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        originScale = transform.localScale;
        facingX = 1;
    }

    void Update()
    {
        PlayerDetect();

        ApplyFlip();

        Skill_A();
        Skill_AVer2();
        Skill_B();
        Skill_CTrackVer();
        Skill_CRandVer();
    }

    bool isGrounded;
    void FixedUpdate()
    {
        if (isGrounded)
        {
            OnJumpLanded();
        }
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

        if(MathF.Abs(distanceOfX) > 0.1f)
        {
            facingX = distanceOfX >0 ? 1: -1;
        }
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
        isGrounded = false;

        yield return new WaitForSeconds(skillADelay);

        rb.AddForce(new Vector2(JumpXForce * -facingX, JumpYForce), ForceMode2D.Impulse);
        StartCoroutine(SkillABackJump());

        yield return new WaitForSeconds(skillAThrowMoment);
        

        yield return new WaitForSeconds(0.5f);
        CachPlayerPos();
        
        ThrowGrenadeEvent();
    }

    IEnumerator SkillABackJump()
    {
        float rotated = 0f;
        float speed = 720f; // 회전 속도

        while (rotated < 360f)
        {
            float step = speed * Time.deltaTime;
            transform.Rotate(0f, 0f, -step);
            rotated += step;
            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void ThrowGrenadeEvent()
    {
        if(!hasCachedTarget) return;

        Vector2 center = cachedTargetPos;

        Vector2 left = center + Vector2.left * sideOffset;
        Vector2 right = center + Vector2.right * sideOffset;

        ThrowGrenade(center);
        //ThrowGrenade(left);
        //ThrowGrenade(right);
        //SkillAGrenadeVer2(center);

        hasCachedTarget = false;
    }

    void ThrowGrenade(Vector2 targetPos)
    {
        //var grenade = Instantiate(holyGrenadePrefab, throwPos.position, Quaternion.identity);
        grenadeTravelTime = 0.3f;
        //grenade.GetComponent<BossLunaHolyGrenade>().InitializeGrenadeThrow(targetPos, grenadeTravelTime, gameObject);
        
        var grenade = Instantiate(grenadeVer2, throwPos.position, Quaternion.identity);
        grenade.GetComponent<GrenadeVer2>().InitializeGrenadeThrow(targetPos, grenadeTravelTime, gameObject);
        hasCachedTarget = false;
    }
    #endregion

    #region Skill_A Ver2
    [SerializeField] float nearDist = 6f;
    [SerializeField] float farDist  = 12f;
    public float jumpTime;
    public float margin;
    bool useStraight;
    Vector2 landingPos;
    void Skill_AVer2()
    {
        if(canUseSkillA_Ver2)
        {
            canUseSkillA_Ver2 = false;
            StartCoroutine(SkillAVer2Routine());
        }
    }

    IEnumerator SkillAVer2Routine()
    {
        isGrounded = false;
        CachPlayerPos();
        SkillAJumpPosCalc();

        yield return new WaitForSeconds(0.7f);

        StartBackJump();

        yield return new WaitForSeconds(0.1f);

        StartCoroutine(SkillABackJump());

        yield return new WaitForSeconds(0.3f);

        //ThrowGrenadeEvent();
        //StartCoroutine(LandFriction(0.1f, 10f));
    }

    void StartBackJump()
    {
        Vector2 start = rb.position;
        Vector2 v = CalcVelocity(start, landingPos, jumpTime);
        rb.linearVelocity = v;
    }

    void SkillAJumpPosCalc()
    {
        Vector2 bossPos = rb.position;  
        float d = Vector2.Distance(bossPos, cachedTargetPos);

        float jumpTime = (d < nearDist) ? jumpTimeNear
            : (d > farDist)  ? jumpTimeFar
            : Mathf.Lerp(jumpTimeNear, jumpTimeFar, (d - nearDist) / (farDist - nearDist));

        useStraight = d > farDist;

        // 착지점: 보는 방향 반대(-facingX)로 플레이어 기준 10f
        float desiredX = cachedTargetPos.x + (-facingX) * 10f;

        Bounds b = bossRoomArea.bounds;
        desiredX = Mathf.Clamp(desiredX, b.min.x + margin, b.max.x - margin);

        landingPos = new Vector2(desiredX, rb.position.y);
    }

    Vector2 CalcVelocity(Vector2 start, Vector2 target, float time)
    {
        Vector2 d = target - start;
        float g = -Physics2D.gravity.y * rb.gravityScale; // 양수

        float vx = d.x / time;
        float vy = (d.y + 0.5f * g * time * time) / time;

        return new Vector2(vx, vy);
    }

    // void SkillAGrenadeVer2(Vector2 tartgetPos)
    // {
    //     if(useStraight)
    //     {
    //         var grenade = Instantiate(grenadeVer2, throwPos.position, Quaternion.identity);
    //         grenadeTravelTime = 0.4f;
    //         grenade.GetComponent<GrenadeVer2>().InitializeGrenadeThrow(targetPos, grenadeTravelTime, gameObject);
    //     }
    //     else
    //     {
    //         var grenade = Instantiate(holyGrenadePrefab, throwPos.position, Quaternion.identity);
    //         grenadeTravelTime = 1.1f;
            
    //         grenade.GetComponent<BossLunaHolyGrenade>().InitializeGrenadeThrow(targetPos, grenadeTravelTime, gameObject);
    //     }
        
        
    //     //grenade.GetComponent<GrenadeVer2>().InitializeGrenadeThrow(targetPos, grenadeTravelTime, gameObject);
    //     hasCachedTarget = false;
    // }
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

    void ApplyFlip()
    {
        gameObject.transform.localScale = new Vector3(facingX > 0 ? -originScale.x : originScale.x, originScale.y, originScale.z);
    }

    void OnJumpLanded()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.angularVelocity = 0f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // IEnumerator LandFriction(float time, float drag)
    // {
    //     float prev = rb.linearDamping;
    //     rb.linearDamping = drag;
    //     yield return new WaitForSeconds(time);
    //     rb.linearDamping = prev;
    // }

    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, aggroRange);
    // }
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BossLuna : MonoBehaviour, IDamageable
{
    public enum bossLunaState { Idle, Aggro, Attack,TakeDamage, Dead };
    public bossLunaState state = bossLunaState.Idle;
    float stateTimer;

    enum Loco { Idle, Aggro }
    Loco loco = Loco.Idle;

    #region Variables
    [SerializeField] Transform playerTransform;
    [SerializeField] Animator animator;
    HP hp;
    LocalSFX localSFX;

    [Header("Detect")]
    [SerializeField] float aggroRange;
    float distanceToPlayer;
    float distanceOfX;
    Vector2 targetPos;
    [SerializeField] LayerMask playerMask;

    [Header("Attack")]
    [SerializeField] GameObject basicAttackHitBox;
    [SerializeField] float attackRange;

    [Header("SkillA")]
    [SerializeField] GameObject holyGrenadePrefab;
    [SerializeField] GameObject grenadeVer2;
    [SerializeField] Transform throwPos;
    [SerializeField] float skillARange;
    [SerializeField] float skillACoolTime;
    [SerializeField] float skillASideOffset; // 투척한 3개의 수류탄 사이의 간격
    [SerializeField] float skillAGrenadeTravelTime;
    [SerializeField] float skillAJumpXForce;
    [SerializeField] float skillAJumpYForce;
     float skillANearJumpDist = 6f;
     float skillAFarJumpDist  = 12f;
    float skillAJumpMargin;
    float skillAFarJumpTime;
    float skillANearJumpTime;
    float nextSkillATime;
    float skillAJumpTime;
    bool skillAUseStraightThrow;
    Vector2 skillALandingPos;

    [Header("SkillB")]
    [SerializeField] GameObject expiationPrefab;
    [SerializeField] float skillBMaxRange;
    [SerializeField] float skillBMinRange;
    [SerializeField] float skillBCoolTime;
    [SerializeField] float skillBDelay;
    [SerializeField] float skillBSpawnTime;
    float nextSkillBTime;

    [Header("SkillC_Track")]
    [SerializeField] GameObject genesisPrefab;
    [SerializeField] float skillCMaxRange;
    [SerializeField] float skillCMinRange;
    [SerializeField] float skillCCoolTime;
    [SerializeField] float skillCSpawnTime;
    [SerializeField] float skillCTrackDuration = 5f;
    [SerializeField] float skillCTrackInterval = 1.2f;
    [SerializeField] float skillCTrackElapsed = 0f;
    float nextSkillCTime;

    [Header("SkillC_Random")]
    [SerializeField] Collider2D bossRoomArea;
    [SerializeField] float skillCRandDuration = 4f;
    [SerializeField] float skillCRandInterval = 0.8f;
    [SerializeField] float skillCminDistance = 2f; 
    [SerializeField] int maxTries = 20;
    [SerializeField] float skillCSpawnY;
    [SerializeField] int skillCRandomPosMemoryCount = 6;

    bool isUsingSkill = false;
    [SerializeField] float delayForNextSkill;

    [Header("OnDamage")]
    [SerializeField] float knockBackXForce;
    [SerializeField] float knockBackYForce;
    [SerializeField] float hitStunTime;
    [SerializeField] float hitLockDuration;
    float hitLockTimer = 0f;
    Vector2 lastHitFrom;
    bool isHit = false;
    public bool isInvincible = false;
    bool isDead = false;
    [SerializeField] int hitTriggerCount = 5;
    [SerializeField] float hitComboWindow = 2f;
    int hitCombo = 0;
    float lastHitTime = -999f;
    bool grenadeQueuedByHits = false;

    [Header("Player Pos Check")]
    public LayerMask groundMask;
    public float groundRayLength;
    Vector2 cachedTargetPos;
    bool hasCachedTarget;

    [SerializeField] MonsterEventChannel monsterEventChannel;

    Rigidbody2D rb;

    int facingX = 1;
    Vector3 originScale;
    #endregion

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        hp = GetComponent<HP>();
        localSFX = GetComponent<LocalSFX>();

        isDead = false;
        isAttacking = false;
        hp.OnDied += OnDied;

        originScale = transform.localScale;
        facingX = 1;

        ChangeState(bossLunaState.Idle);
    }

    public void OnDestroy()
    {
        if(hp != null) hp.OnDied -= OnDied;        
    }

    void Update()
    {
        PlayerDetect();

        stateTimer += Time.deltaTime;
        StateMachine();
        SelectPattern();
        ApplyFlip();
        
        ResetHitComboIfExpired();
        hitLockTimer -= Time.deltaTime;
    }

    void StateMachine()
    {
        switch(state)
        {
            case bossLunaState.Idle: TickIdle(); break;
            case bossLunaState.Aggro: TickAggro(); break;
            case bossLunaState.Attack: break;
            case bossLunaState.TakeDamage: TickTakeDamage(); break;
            case bossLunaState.Dead: break;
        }
    }

    void ChangeState(bossLunaState nextState)
    {
        state = nextState;
        stateTimer = 0f;

        if(state != bossLunaState.TakeDamage && isHit) isHit = false;
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

    void SelectPattern()
    {   
        if(isHit || state == bossLunaState.TakeDamage) return;
        if(isUsingSkill || isAttacking) return;

        if(canUsePatternC() && !hasUsedPatternC)
        {
            hasUsedPatternC = true;
            StartCoroutine(PatternC());
            //Debug.Log("PatternC");
            return;
        }

        if(specialQueuedByAttackHit)
        {
            specialQueuedByAttackHit = false;
            StartCoroutine(PatternA());
            //Debug.Log("PatternA");
            return;
        }

        if(grenadeQueuedByHits)
        {
            grenadeQueuedByHits = false;
            StartCoroutine(SkillARoutine());
            //Debug.Log("SkillA");
            return;
        }

        if (!randomSkillQueued && Time.time >= nextRandomSkillTime)
        {
            StartCoroutine(RandomSkillRoutine());
        }

        if(distanceToPlayer < attackRange && canAttack && !isAttacking)
        {
            TickAttack();
            return;
        }
    }

    [SerializeField] float gapA = 5f;
    [SerializeField] float gapB = 8f;
    [SerializeField] float gapC = 10f;
    IEnumerator RandomSkillRoutine()
    {
        randomSkillQueued = true;

        // 거리 / 체력비율
        float dist = distanceToPlayer;
        float hpRatio = hp.CurHP / hp.MaxHP;

        int pick = PickSkillNoRepeat(dist, hpRatio);

        // 후보가 없으면 그냥 종료(기본 행동으로 빠지게)
        if (pick == -1)
        {
            randomSkillQueued = false;
            yield break;
        }

        if (pick == 0) nextRandomSkillTime = Time.time + gapA;
        else if (pick == 1) nextRandomSkillTime = Time.time + gapB;
        else nextRandomSkillTime = Time.time + gapC;

        // 실행
        if (pick == 0) yield return StartCoroutine(SkillARoutine());
        else if (pick == 1) yield return StartCoroutine(SkillBRoutine());
        else
        {
            if (firstTimeUsingSkillC)
            {
                firstTimeUsingSkillC = false;
                yield return StartCoroutine(SkillCTrackRoutine());
            }
            else
            {
                yield return StartCoroutine(SkillCRandomRoutine());
            }
        }

        randomSkillQueued = false;
    }

    int lastPick = -1;
    int PickSkillNoRepeat(float dist, float hpRatio)
    {
        if (Time.time < nextRandomSkillTime) return -1;

        int[] pool = new int[3];
        int count = 0;

        if (dist <= skillARange && CanUseSkillA())
            pool[count++] = 0;
    
        if (dist >= skillBMinRange && dist <= skillBMaxRange && CanUseSkillB())
            pool[count++] = 1;

        if (hpRatio <= 0.6f &&
            dist >= skillCMinRange && dist <= skillCMaxRange && CanUseSkillC())
            pool[count++] = 2;

        if (count == 0) return -1;

        if (count > 1)
        {
            int tmpCount = 0;
            for (int i = 0; i < count; i++)
            if (pool[i] != lastPick)
                pool[tmpCount++] = pool[i];

            if (tmpCount > 0)
                count = tmpCount;
        }

        int pick = pool[UnityEngine.Random.Range(0, count)];
        lastPick = pick;
        return pick;
    }

    void TickIdle()
    {
        StopX();

        if(distanceToPlayer < 20f && !isHit && !isAttacking && !isUsingSkill)
        {
            ChangeState(bossLunaState.Aggro);
            animator?.SetTrigger("Aggro");
            return;
        }
    }

    #region Aggro
    [SerializeField] float speed;
    void TickAggro()
    {
        if (isAttacking || isUsingSkill) return;

        float absX = Mathf.Abs(distanceOfX);

        if (absX <= 0.5f)
        {
            StopX();
            SetLocomotion(Loco.Idle);

            return;
        }

        facingX = distanceOfX > 0 ? 1 : -1;
        MoveX(facingX, speed);
        SetLocomotion(Loco.Aggro);
    }
    #endregion

    void SetLocomotion(Loco next)
    {
        if (loco == next) return;
        loco = next;

        animator?.ResetTrigger("Idle");
        animator?.ResetTrigger("Aggro");

        animator?.SetTrigger(next == Loco.Idle ? "Idle" : "Aggro");
    }

    #region Attack
    [SerializeField] int attackHitTriggerCount = 2;
    [SerializeField] float attackComboWindow = 5f;
    bool canAttack = true;
    bool isPatternAttack = false;
    public int attackHitCombo = 0;
    float lastAttackHitTime = -999f;
    bool specialQueuedByAttackHit = false;
    bool isAttacking;
    void TickAttack()
    {
        isAttacking = true;
        ChangeState(bossLunaState.Attack);
        canAttack = false;
        StopX();
        animator?.SetTrigger("Attack");
    }

    public void OnAttackHitBoxOn()
    {
        basicAttackHitBox.SetActive(true);
    }

    public void OnAttackHitBoxOff()
    {
        basicAttackHitBox.SetActive(false);
        isAttacking = false;

        if (isPatternAttack)
        {
            isPatternAttack = false;
            return;
        }

        StartCoroutine(AttackCoolDown());
    }

    IEnumerator AttackCoolDown()
    {
        animator?.SetTrigger("Idle");
        ChangeState(bossLunaState.Idle);
        yield return new WaitForSeconds(2f);
        
        canAttack = true;
    }

    public void RegisterBasicAttackHit()
    {
        float now = Time.time;

        if (now - lastAttackHitTime > attackComboWindow)
            attackHitCombo = 0;

        lastAttackHitTime = now;
        attackHitCombo++;

        if (attackHitCombo >= attackHitTriggerCount)
        {
            attackHitCombo = 0;
            specialQueuedByAttackHit = true;
        }
    }
    
    void TickAttack_Pattern()
    {
        if (isAttacking) return;

        isPatternAttack = true;
        isAttacking = true;

        ChangeState(bossLunaState.Attack);
        StopX();

        animator?.SetTrigger("Attack");
    }
    #endregion

    #region Skill_A
    bool isBackJumping;
    IEnumerator SkillARoutine()
    {
        nextSkillATime = Time.time + skillACoolTime;

        ChangeState(bossLunaState.Attack);
        isUsingSkill = true;
        StopX();

        yield return new WaitForSeconds(0.2f);

        animator?.SetTrigger("BackFlip");
        rb.AddForce(new Vector2(skillAJumpXForce * -facingX, skillAJumpYForce), ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.8f);

        CachPlayerPos();
        ThrowGrenadeEvent();
        isUsingSkill = false;
        animator?.SetTrigger("Idle");
        yield return new WaitForSeconds(1f);
        
        ChangeState(bossLunaState.Aggro);
        animator?.SetTrigger("Aggro");
    }

    void ThrowGrenadeEvent()
    {
        if(!hasCachedTarget) return;

        Vector2 center = cachedTargetPos;

        Vector2 left = center + Vector2.left * skillASideOffset;
        Vector2 right = center + Vector2.right * skillASideOffset;

        SkillAGrenadeInstantiate(center);
        // SkillAGrenadeInstantiate(left);
        // SkillAGrenadeInstantiate(right);

        hasCachedTarget = false;
    }

    void SkillAGrenadeInstantiate(Vector2 targetPos)
    {
        var grenade = Instantiate(holyGrenadePrefab, throwPos.position, Quaternion.identity);
        grenade.GetComponent<BossLunaHolyGrenade>().InitializeGrenadeThrow(targetPos, skillAGrenadeTravelTime, gameObject);
        
        hasCachedTarget = false;
    }

    void SkillAGrenadeInstantiateVer2(Vector2 targetPos)
    {
        targetPos = new Vector2(cachedTargetPos.x, cachedTargetPos.y);
        var grenade = Instantiate(grenadeVer2, throwPos.position, Quaternion.identity);
        grenade.GetComponent<GrenadeVer2>().InitializeGrenadeThrow(targetPos, skillAGrenadeTravelTime, gameObject);

        hasCachedTarget = false;
    }
    #endregion

    #region Skill_B
    IEnumerator SkillBRoutine()
    {   
        ChangeState(bossLunaState.Attack);
        nextSkillBTime = Time.time + skillBCoolTime;

        StopX();
        
        animator?.SetTrigger("Expiation");
        isUsingSkill = true;
        yield return new WaitForSeconds(skillBDelay);

        CachPlayerPos();

        ExpiationEvent();
        localSFX.Play("CastExpiation");

        yield return new WaitForSeconds(2f);

        if(hasSkillBHit) StartCoroutine(PatternB());
        else
        {
            isUsingSkill = false;
            ChangeState(bossLunaState.Idle);
            animator?.SetTrigger("Idle");
        }
    }

    void ExpiationEvent()
    {
        if(!hasCachedTarget) return;
        Vector2 expiationSpawnpoint = cachedTargetPos;

        var expiation = Instantiate(expiationPrefab, expiationSpawnpoint, expiationPrefab.transform.rotation);
        expiation.GetComponent<BossLunaExpiation>().InitializeExpiation(expiationSpawnpoint, skillBSpawnTime, this);

        hasCachedTarget = false;
    }
    #endregion

    #region Skill_C
    void GenesisEvent()
    {
        if(!hasCachedTarget) return;
        Vector2 genesisSpawnpoint = cachedTargetPos + Vector2.up * genesisPrefab.transform.position.y;

        var genesis = Instantiate(genesisPrefab, genesisSpawnpoint, genesisPrefab.transform.rotation);
        genesis.GetComponent<BossLunaGenesis>().InitializeGenesis(genesisSpawnpoint, skillCSpawnTime, gameObject);
        localSFX.Play("Genesis");

        hasCachedTarget = false;
    }

    IEnumerator SkillCTrackRoutine()
    {
        StopX();
        nextSkillCTime = Time.time + skillCCoolTime;
        
        ChangeState(bossLunaState.Attack);
        animator?.SetTrigger("Genesis");
        isUsingSkill = true;
        isInvincible = true;

        skillCTrackElapsed = 0f;
        while (skillCTrackElapsed < skillCTrackDuration)
        {
            CachPlayerPos();

            GenesisEvent();

            yield return new WaitForSeconds(skillCTrackInterval);

            skillCTrackElapsed += skillCTrackInterval;
        }
        

        isInvincible = false;
        yield return new WaitForSeconds (2f);
        isUsingSkill = false;

        ChangeState(bossLunaState.Aggro);
        animator?.SetTrigger("Aggro");
    }

    IEnumerator SkillCRandomRoutine()
    {
        ChangeState(bossLunaState.Attack);
        animator?.SetTrigger("Genesis");
        isUsingSkill = true;
        isInvincible = true;
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


        if(isUsingPatternC) 
        {
            StopCoroutine(SkillCRandomRoutine());

            isInvincible = false;
        }
        else
        {
            isUsingSkill = false;
            isInvincible = false;
            ChangeState(bossLunaState.Idle);
            animator?.SetTrigger("Idle");
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
    #endregion

    #region Teleport
    public Collider2D bossCollider;
    public float teleportXDistance = 8f;
    public float teleportPadding = 0.1f;
    void TeleportFromPlayer()
    {
        Bounds b = bossRoomArea.bounds;

        float bossRoomHalf = bossCollider.bounds.extents.x;

        float minX = b.min.x + bossRoomHalf + teleportPadding;
        float maxX = b.max.x - bossRoomHalf - teleportPadding;

        float curX = rb ? rb.position.x : transform.position.x;
        float y = rb ? rb.position.y : transform.position.y;

        //플레이어 반대 방향으로 이동
        float dir = (curX < playerTransform.position.x) ? -1f : 1f;
        float candidateX = curX + dir * teleportXDistance;

        //방 밖이면 플레이어 건너편으로 스왑
        if (candidateX < minX || candidateX > maxX)
        {
            float swapDir = (curX < playerTransform.position.x) ? 1f : -1f;
            candidateX = playerTransform.position.x + swapDir * teleportXDistance;
        }

        //무조건 방 안으로
        candidateX = Mathf.Clamp(candidateX, minX, maxX);

        SetX(candidateX);
    }

    void SetX(float x)
    {
        if (rb) rb.position = new Vector2(x, rb.position.y);
        else transform.position = new Vector2(x, transform.position.y);
    }

    void TeleportToPlayer()
    {
        // 텔포 거리 계산 측정 방식 수정 하면 될듯 ? maybe?
        Bounds b = bossRoomArea.bounds;

        float bossRoomHalf = bossCollider.bounds.extents.x;

        float minX = b.min.x + bossRoomHalf + teleportPadding;
        float maxX = b.max.x - bossRoomHalf - teleportPadding;

        float offSet = 1f;
        float playerPos = cachedTargetPos.x;

        float candidateX = playerPos + offSet;

        //방 밖이면 플레이어 건너편으로 스왑
        if (candidateX < minX || candidateX > maxX)
        {
            candidateX = playerPos - offSet;
        }

        //무조건 방 안으로
        candidateX = Mathf.Clamp(candidateX, minX, maxX);

        SetX(candidateX);
    }
    #endregion

    #region PatternA
    IEnumerator PatternA()
    {
        ChangeState(bossLunaState.Attack);
        isUsingSkill = true;
        facingX = distanceOfX > 0 ? 1 : -1;
        TickAttack_Pattern();

        yield return new WaitForSeconds(1f);

        StopX();
        isInvincible = true;
        animator?.SetTrigger("Idle");
        CachPlayerPos();
        TeleportToPlayer();

        yield return new WaitForSeconds(0.7f);

        StopX();
        isInvincible = false;
        facingX = distanceOfX > 0 ? 1 : -1;
        TickAttack_Pattern();

        yield return new WaitForSeconds(1f);
        
        StopX();
        isInvincible = true;
        animator?.SetTrigger("Idle");
        CachPlayerPos();
        TeleportFromPlayer();

        yield return new WaitForSeconds(1.5f);
        StopX();
        isInvincible = false;

        CachPlayerPos();
        facingX = distanceOfX > 0 ? 1 : -1;
        animator?.SetTrigger("GrenadeThrow");
        SkillAGrenadeInstantiateVer2(cachedTargetPos);
        
        yield return new WaitForSeconds(1f);
        StopX();
        grenadeQueuedByHits = false;
        isUsingSkill = false;

        ChangeState(bossLunaState.Idle);
        animator?.SetTrigger("Idle");
        
    }
    #endregion

    #region PatternB
    public bool hasSkillBHit = false;
    IEnumerator PatternB()
    {
        StopX();
        yield return new WaitForSeconds(0.3f);
        CachPlayerPos();
        Vector2 throwPos = new Vector2(cachedTargetPos.x, cachedTargetPos.y + 0.5f);
        facingX = distanceOfX > 0 ? 1 : -1;
        animator?.SetTrigger("GrenadeThrow");
        SkillAGrenadeInstantiateVer2(throwPos);

        yield return new WaitForSeconds(1f);

        hasSkillBHit = false;
        isUsingSkill = false;
        ChangeState(bossLunaState.Idle);
        animator?.SetTrigger("Idle");
    }
    #endregion

    #region PatternC
    bool isUsingPatternC = false;
    IEnumerator PatternC()
    {   
        isUsingSkill = true;
        hasUsedPatternC = true;
        isUsingPatternC = true;
        ChangeState(bossLunaState.Attack);
        animator?.SetTrigger("Expiation");
        
        StopX();
        isInvincible = true;

        yield return new WaitForSeconds(1f);
        
        StopX();
        CachPlayerPos();
        ExpiationEvent();

        yield return new WaitForSeconds(3f);

        StopX();
        facingX = distanceOfX > 0 ? 1 : -1;
        animator?.SetTrigger("GrenadeThrow");
        CachPlayerPos();
        ThrowGrenadeEvent();

        yield return new WaitForSeconds(2f);

        animator?.SetTrigger("Genesis");
        StartCoroutine(SkillCRandomRoutine());

        yield return new WaitForSeconds(10f);
        isUsingSkill = false;
        ChangeState(bossLunaState.Idle);
        animator?.SetTrigger("Idle");
        
    }
    #endregion

    void CachPlayerPos()
    {
        if(!playerTransform) { hasCachedTarget = false; return;}

        Vector2 origin = (Vector2)playerTransform.position + Vector2.up * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundMask);

        Debug.DrawRay(origin, Vector2.down * groundRayLength, Color.yellow, 2f);

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

    void MoveX(int dirX, float speed)
    {
        rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
    }

    void StopX()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    bool CanUseSkillA() => Time.time >= nextSkillATime;
    bool CanUseSkillB() => Time.time >= nextSkillBTime;
    bool firstTimeUsingSkillC = true;
    bool CanUseSkillC()
    {
        if (hp.CurHP / hp.MaxHP > 0.6f) return false;
        if (Time.time < nextSkillCTime) return false;
        return true;
    }

    bool hasUsedPatternC;
    bool canUsePatternC()
    {
        if(hp.CurHP / hp.MaxHP > 0.2f) return false;
        return true;
    }

    float nextRandomSkillTime = 0f;
    [SerializeField] float randomSkillGap = 2.0f;
    bool randomSkillQueued = false;

    #region Damage Control
    void TickTakeDamage()
    {
        //if(isInvincible || isAttacking || isUsingSkill) return;

        if(stateTimer >= hitStunTime)
        {
            isHit = false;
            hitLockTimer = hitLockDuration;
            ChangeState(bossLunaState.Idle);
            animator?.SetTrigger("Idle");
        }
    }

    public void TakeDamage(float amount)
    {
        if(isInvincible) return;

        hp.TakeDamage(amount);
        OnDamagedForCombo();

        if(isUsingSkill || isDead || isAttacking) return;

        OnHit(transform.position - Vector3.right);
    }

    void IDamageable.TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        if(isInvincible) return;

        hp.TakeDamage(amount);
        OnDamagedForCombo();

        if(isUsingSkill || isDead || isAttacking) return;

        lastHitFrom = (Vector2)attackerWorldPosition;
        OnHit((Vector2)attackerWorldPosition);
    }

    public virtual void OnHit(Vector2 attackWorldPosition)
    {
        ChangeState(bossLunaState.TakeDamage);
        animator?.SetTrigger("TakeDamage");
        isHit = true;

        Vector2 dir = ((Vector2)transform.position - attackWorldPosition).normalized;
        dir = new Vector2(dir.x * knockBackXForce,knockBackYForce);
        rb.linearVelocity = dir; 
    }

    public void OnDamagedForCombo()
    {
        if (state == bossLunaState.Dead) return;

        float now = Time.time;

        if (now - lastHitTime > hitComboWindow)
        hitCombo = 0;

        lastHitTime = now;
        hitCombo++;

        if (hitCombo >= hitTriggerCount)
        {
            hitCombo = 0;
            grenadeQueuedByHits = true; 
        }
    }

    void ResetHitComboIfExpired()
    {
        if (hitCombo <= 0) return;

        if (Time.time - lastHitTime > hitComboWindow)
            hitCombo = 0;
    }

    public virtual void OnDied()
    {
        if(isDead) return;

        StopX();
        isDead = true;

        ChangeState(bossLunaState.Dead);
        animator?.SetTrigger("Dead");
        isUsingSkill = false;

        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        monsterEventChannel.RaiseMonsterDead(MonsterType.Luna);
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, skillARange);

        // Gizmos.color = Color.green;
        // Gizmos.DrawWireSphere(transform.position, skillBRange);

        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(transform.position, skillCRange);
    }
}

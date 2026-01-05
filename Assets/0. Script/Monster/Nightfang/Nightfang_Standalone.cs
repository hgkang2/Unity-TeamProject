using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class NightfangStandalone : MonoBehaviour, IDamageable
{
    [Header("Debug Test")]
    public bool enablePatrol = false;
    public bool enableAggro = false;
    public bool enableAttack = false;
    public bool enableSkill = false;

    public enum State { Idle, Patrol, Aggro, Attack, Skill, TakeDamage, Dead }
    [Header("State")]
    public State state = State.Idle;
    public float stateTimer;

    #region Variables
    [Header("Refs")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] public SpriteRenderer spriteRenderer;
    LocalSFX sfx;
    HP hp;

    [Header("Hitbox")]
    [SerializeField] public GameObject skillHitBoxObj;   // 스킬 공격시 사용하는 히트박스
    [SerializeField] public GameObject attackHitboxObj;
    [SerializeField] BoxCollider2D boxCollider;

    [Header("Idle")]
    [SerializeField] float idleTime = 1.0f;       // Idle(대기)상태 지속 시간

    [Header("Patrol")]
    [SerializeField] float patrolSpeed = 2.0f;    // Patrol(순찰) 이동 속도
    [SerializeField] float patrolTime = 2.0f;     // Patrol(순찰) 시간 시간

    [Header("Aggro")]
    [SerializeField] float aggroRange = 6.0f;     // Aggro 범위
    [SerializeField] float aggroSpeed = 3.5f;     // Aggro 상태 이동 속도
    [SerializeField] float maxHeightDiffForAttack = 0.8f;
    int moveDirX = 1;

    [Header("Attack")]
    [SerializeField] float attackRange = 1.2f;    
    [Tooltip("일반 공격 선딜레이")]
    [SerializeField] float readyAttackWindup = 1f;
    [SerializeField] float attackCoolTime = 1.0f;
    [Tooltip("일반 공격 지속 시간")]
    [SerializeField] float attackDuration = 1f;
    bool isAttacking;
    bool isAttackReady = true;

    [Header("Skill")]
    [SerializeField] float skillRange = 3.5f;
    [Tooltip("스킬 선딜레이")]
    [SerializeField] float readySkillWindup = 0.25f;
    [SerializeField] float skillCoolTime = 0.2f;
    [Tooltip("스킬 지속 시간")]
    [SerializeField] float skillDuration;
    bool isUsingSkill;
    bool isSkillReady = true;

    [Header("딜레이 시간")]
    [Tooltip("공격(스킬 or 일반공격)시전 후 다음 공격까지 필요한 시간")]
    [SerializeField] float delayTime;
    [Tooltip("공격(스킬 or 일반공격)시전 후 정지 상태로 대기하는 시간")]
    [SerializeField] float standByTime;
    bool isActionLocked = false; // 연속 공격 방지
    bool canMove = true;    // 대기 도중 이동 방지

    [Header("Hit")]
    [Tooltip("몹이 피격 시 경직에 걸리는 시간")]
    [SerializeField] float hitStunTime = 0.25f;
    [SerializeField]float knockBackXForce = 5f;
    [SerializeField]float knockBackYForce = 1f;
    float hitLockTimer = 0f;
    [SerializeField] float hitLockDuration = 0.15f;
    public bool isHit;
    Vector2 lastHitFrom;

    [Header("Detect")]
    [SerializeField] Transform player;
    [SerializeField] LayerMask playerMask;
    [Tooltip("플레이어까지의 거리")]
    [SerializeField] float distance;

    [Header("FreezeZone")]
    [Tooltip("플레이어와 거리가 freezeZoneX보다 작을 경우 몹의 이동 제한")]
    [SerializeField] float freezeZoneX = 0.1f;
    float dx;  // 좌우 방향 + 좌우 거리
    
    // facing/flip 
    int facingX = 1;
    Vector3 originScale;

    // 사망 여부
    bool isDead;

    // 코루틴 중복 방지
    Coroutine runningRoutine;
    Coroutine lockActionRoutine; // 공격(스킬 or 일반 공격) 후 연속 공격 방지

    bool heightOk;
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform rayOrigin;
    [SerializeField] bool isCliffAhead;
    #endregion

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!hp) hp = GetComponent<HP>();
        if (!sfx) sfx = GetComponent<LocalSFX>();

        isDead = false;

        hp.OnDied += OnDied;

        originScale = transform.localScale;
        facingX = 1;
        moveDirX = 1;

        Physics2D.queriesStartInColliders = false;

        if (skillHitBoxObj) skillHitBoxObj.SetActive(false);
        if (attackHitboxObj) attackHitboxObj.SetActive(false);

        ChangeState(State.Idle);
    }

    public void OnDestroy()
    {
        hp.OnDied -= OnDied;
    }

    void Update()
    {
        if (isDead) return;

        PlayerDetect();

        stateTimer += Time.deltaTime;
        RunFSM();
        
        ApplyFlip();
        MonsterGroundCheck();
        hitLockTimer -= Time.deltaTime;
    }

    void PlayerDetect()
    {
        if (!player)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, aggroRange, playerMask);

            if(hit)
                player = hit.transform;
            else
            {
                distance = float.PositiveInfinity;
                dx = 0f;
                return;
            }
        }

        if(!player.gameObject.activeInHierarchy)
        {
            player = null;
            distance = float.PositiveInfinity;
            dx = 0f;
            return;
        }

        Vector2 myPos = transform.position;
        Vector2 playerPos = player.position;

        Vector2 toPlayer = playerPos - myPos;
        dx = toPlayer.x;
        distance = toPlayer.magnitude;

        if(state == State.Aggro)
        {
            if (Mathf.Abs(dx) > freezeZoneX)
            moveDirX = dx > 0f ? 1 : -1;
        }
    }
    
    void RunFSM()
    {
        switch (state)
        {
            case State.Idle: TickIdle();; break;
            case State.Patrol: TickPatrol(); break;
            case State.Aggro:TickAggro(); break;
            case State.Attack: if(cliffStopped) {StopX(); animator?.SetBool("Aggro_Idle", true); } break;
            case State.Skill: if(cliffStopped) {StopX(); animator?.SetBool("Aggro_Idle", true); } break;
            case State.TakeDamage : TickTakeDamage(); break;
            case State.Dead: break;
        }
    }

    void TickIdle()
    {
        StopX();
        
        if (!enablePatrol && !enableAggro) return;

        if(!canMove) return;

        if (enablePatrol && stateTimer >= idleTime && !isHit)
        {
            moveDirX *= -1;
            ApplyFlip();
            ChangeState(State.Patrol);
            animator?.SetTrigger("Patrol");
            return;
        }

        if (enableAggro && distance <= aggroRange && !isHit)
        {
            ChangeState(State.Aggro);
            animator?.SetTrigger("Aggro");
            return;
        }
    }

    void TickPatrol()
    {
        if (stateTimer >= patrolTime)
        {
            StopX();
            animator?.SetTrigger("Idle");
            ChangeState(State.Idle);
            sfx.Play("IdleSound");
            return;
        }

        if (enableAggro && distance <= aggroRange)
        {
            StopX();
            ChangeState(State.Aggro);
            animator?.SetTrigger("Aggro");
            return;
        }

        if(cliffStopped)
        {
            StopX();
            animator?.SetBool("Aggro_Idle", true);
            return;
        }

        
        MoveX(moveDirX, patrolSpeed);
        facingX = moveDirX;
    }

    void TickAggro()
    {
        float dy = player ? Mathf.Abs(player.position.y - transform.position.y) : float.PositiveInfinity;

        heightOk = dy <= maxHeightDiffForAttack;

        if (hitLockTimer > 0f)
        {
            StopX();                 
            return;
        }

        if (!isAttacking && !isUsingSkill)
        {
            float deadZone = 0.05f;
            if (Mathf.Abs(dx) > deadZone) facingX = moveDirX;
        }

        if (distance >= aggroRange)
        {
            ChangeState(State.Idle);
            animator?.SetTrigger("Idle");
            sfx.Play("IdleSound");
            return;
        }

        if(cliffStopped)
        {
            StopX();
            animator?.SetBool("Aggro_Idle", true);

            return;
        }

        MoveX(moveDirX, aggroSpeed);

        if (enableSkill && heightOk &&
            distance <= skillRange &&
            distance >= attackRange && !isActionLocked &&
            isSkillReady && !isUsingSkill && !isHit && !cliffStopped)
        {
            StartSkill();
            return;
        }

        if (enableAttack && heightOk &&
            distance <= attackRange && !isActionLocked &&
            isAttackReady && !isAttacking && !isHit && !cliffStopped)
        {
            StartAttack(); 
            return;
        }
    }

    #region Attack / Skill
    void StartAttack()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        if (lockActionRoutine != null) StopCoroutine(lockActionRoutine);
        isAttacking = true;
        isAttackReady = false;
        isActionLocked = true;
        canMove = false;
        animator?.SetTrigger("ReadySkill");
        runningRoutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        ChangeState(State.Attack);
        StopX();
        yield return new WaitForSeconds(readyAttackWindup);

        sfx.Play("AttackSound");
        animator?.SetTrigger("Attack");
        rb.AddForce(7f * Vector2.right * facingX, ForceMode2D.Impulse);

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        StopX();
        ChangeState(State.Idle);
        animator?.SetTrigger("Idle");

        lockActionRoutine = StartCoroutine(LockActionRoutine());
        StartCoroutine(AttackCoolDown());

        yield return new WaitForSeconds(standByTime);
        
        canMove = true;
    }

    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(attackCoolTime);
        isAttackReady = true;
    }

    public void StartSkill()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        isUsingSkill = true;
        isSkillReady = false;
        isActionLocked = true;
        canMove = false;
        animator?.SetTrigger("ReadySkill");
        runningRoutine = StartCoroutine(SkillRoutine());
    }

    IEnumerator SkillRoutine()
    {
        ChangeState(State.Skill);
        
        StopX();

        yield return new WaitForSeconds(readySkillWindup);

        sfx.Play("AttackSound");
        animator?.SetTrigger("Skill");
        rb.AddForce(10f * Vector2.right * facingX, ForceMode2D.Impulse);

        yield return new WaitForSeconds(skillDuration);

        isUsingSkill = false;
        StopX();
        
        ChangeState(State.Idle);
        animator?.SetTrigger("Idle");

        lockActionRoutine = StartCoroutine(LockActionRoutine());
        StartCoroutine(SkillCooldownRoutine());

        yield return new WaitForSeconds(standByTime);
        canMove = true;
    }

    IEnumerator SkillCooldownRoutine()
    {
        yield return new WaitForSeconds(skillCoolTime);
        isSkillReady = true;
    }

    IEnumerator LockActionRoutine()
    {
        yield return new WaitForSeconds(delayTime);
        isActionLocked = false;
    }
    #endregion

    public void ChangeState(State next)
    {
        if (isDead && next != State.Dead) return;

        //Debug.Log($"STATE: {state} -> {next}");
        state = next;
        stateTimer = 0f;
        
        if(state != State.TakeDamage && isHit)
        {
            isHit = false;
        }
    }

    void ApplyFlip()
    {
        // flip
        transform.localScale = new Vector3(
            facingX < 0 ? -originScale.x : originScale.x,
            originScale.y,
            originScale.z
        );
    }

    #region Movement
    void StopX()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void MoveX(int dirX, float speed)
    {
        rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Gizmos.color = Color.green;
        // Gizmos.DrawWireSphere(transform.position, attackRange);

        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(transform.position, skillRange);
        
        
    }

    #region Damage Control
    void TickTakeDamage()
    {
        if(isAttacking || isUsingSkill) return;

        if (stateTimer >= hitStunTime)
        {
            hitLockTimer = hitLockDuration;
            ChangeState(State.Idle);
            animator?.SetTrigger("Idle");
        }
    }

    public void TakeDamage(float amount)
    {
        hp.TakeDamage(amount);
        sfx.Play("HitSound");

        if (isAttacking || isUsingSkill || isDead) return;

        OnHit(transform.position - Vector3.right);
    }

    void IDamageable.TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        hp.TakeDamage(amount);
        sfx.Play("HitSound");
        if (isAttacking || isUsingSkill || isDead) return;

        lastHitFrom = (Vector2)attackerWorldPosition;
        OnHit((Vector2)attackerWorldPosition);
    }

    public virtual void OnHit(Vector2 attackerWorldPosition)
    {
        ChangeState(State.TakeDamage);
        animator?.SetTrigger("Hit");

        Vector2 dir = ((Vector2)transform.position - attackerWorldPosition).normalized;
        dir = new Vector2(dir.x * knockBackXForce, knockBackYForce);

        if(isCliffAhead) return;
        rb.linearVelocity = dir;
    }
    #endregion

    public virtual void OnDied()
    {
        if (isDead) return;

        StopX();
        isDead = true;

        ChangeState(State.Dead);
        animator.SetTrigger("Dead");

        StopAllCoroutines();
        isUsingSkill = false;

        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        FindFirstObjectByType<Player>().Exp.AddExp(10);

        GameObject.Destroy(this.gameObject, 3f);
    }

    bool cliffStopped = false;
    [SerializeField] float rayLength;
    [SerializeField] float forwardPadding;
    [SerializeField] float bottomPadding;

    void MonsterGroundCheck()
    {
        if(state == State.Dead) return;

        Bounds b = boxCollider.bounds;

        float dirX;

        // lastHitFrom 기반으로 밀려나는 방향 쪽으로 레이 시작점 이동
        if (state == State.TakeDamage)
        {
            float dx = ((Vector2)transform.position - lastHitFrom).x;
            dirX = Mathf.Sign(dx);
            if (dirX == 0f) dirX = 1f; // 안전장치
        }
        else
        {
            dirX = Mathf.Sign(facingX);
            if (dirX == 0f) dirX = 1f;
        }

        Vector2 rayStart = new Vector2((dirX > 0f ? b.max.x : b.min.x) + dirX * forwardPadding,b.min.y + bottomPadding);

        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, rayLength, groundMask);

        Debug.DrawRay(rayStart, Vector2.down * rayLength, Color.red);

        isCliffAhead = (hit.collider == null);

        cliffStopped = isCliffAhead;

        if(state == State.TakeDamage) return;
        animator?.SetBool("Aggro_Idle", cliffStopped);
    }
}

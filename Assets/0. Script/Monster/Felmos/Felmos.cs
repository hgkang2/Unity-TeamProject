using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public class Felmos : MonoBehaviour, IDamageable
{
    [Header("Test")]
    public bool enablePatrol = true;
    public bool enableAggro = true;
    public bool enableShoot = true;
    public bool enableKeepDistance = true;
    public bool enableMinHeightLimit = true;

    public enum Felmos_State { Idle, Patrol, Alerted, Aggro, Attack, TakeDamage,Dead }
    [Header("States")]
    public Felmos_State state = Felmos_State.Idle;
    public float stateTimer;

    #region Variables
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] Animator vfxAnimator;
    [SerializeField] MonsterEventChannel monsterEvent;
    HP hp;
    LocalSFX sfx;

    [Header("Idle")]
    [SerializeField] float idleTime;

    [Header("Patrol")]
    [SerializeField] float patrolSpeed;
    [SerializeField] float patrolTime;
    int patrolDirIndex = 0;
    private readonly Vector2[] patrolDirs = new Vector2[]
    {
        Vector2.left, Vector2.right, Vector2.up, Vector2.down
    };

    [Header("Alert")]
    [SerializeField] GameObject alertSprite;
    [SerializeField] float alertedStateDuration;
    float offGuardTimer = 0f;

    [Header("Aggro")]
    [SerializeField] float aggroRange;
    [SerializeField] float aggroSpeed;
    [SerializeField] float freezeZoneX;

    [Header("Attack")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject WarningVFX;
    [SerializeField] private Transform firePos;
    [SerializeField] float bulletDamage = 10f;
    [SerializeField] float attackDuration;
    [SerializeField] float readyAttackWindup;
    [SerializeField] float attackCoolTime;
    [SerializeField] float attackRange = 6f;
    bool isAttackReady = true;
    bool isAttacking = false;

    [Header("딜레이 시간")]
    [SerializeField] float delayTime;
    [SerializeField] float standByTime;

    [Header("Detect Player")]
    [SerializeField] Transform playerTransform;
    [SerializeField] LayerMask playerMask;
    [SerializeField] float distanceToPlayer;
    float distanceOfX;
    float distanceOfY;    

    [Header("Min Height Limit Ray")]
    [SerializeField] Transform groundRayOrigin;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckRayLength;

    [Header("거리 유지")]
    [SerializeField] float waitBeforeRetreat;
    [SerializeField] float retreatDuration;
    [SerializeField] float retreatStartDistance = 4f;      
    [SerializeField] float retreatSpeed; 
    [SerializeField] float retreatCooldown = 0.25f; 
    Coroutine retreatRoutine;
    bool isRetreating;
    float retreatCooldownTimer;

    [Header("대미지 처리")]
    [SerializeField] float hitStunTime;
    [SerializeField] float hitLockTimer;
    [SerializeField] float hitLockDuration;
    Vector2 lastHitFrom;
    [SerializeField] float knockBackXForce;
    [SerializeField] float knockBackYForce;
    bool isDead;
    bool isHit = false;

    //이동
    Vector2 dirToPlayer;    
    int facingX = 1;
    Vector3 originScale;
    bool canMove = true;

    Coroutine runningRoutine;
    #endregion

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!hp) hp = GetComponent<HP>();
        if (!sfx) sfx = GetComponent<LocalSFX>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        hp.OnDied += OnDied;

        originScale = transform.localScale;
        facingX = -1;

        ChangeState(Felmos_State.Idle);
    }

    public void Oestroy()
    {
        hp.OnDied -= OnDied;        
    }

    void Update()
    {
        if (isDead) return;

        UpdateDetect();

        stateTimer += Time.deltaTime;
        RunFSM();

        ApplyFlip();

        hitLockTimer -= Time.deltaTime;

        if (retreatCooldownTimer > 0f)
        retreatCooldownTimer -= Time.deltaTime;
    }

    void UpdateDetect()
    {
        if (!playerTransform)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, aggroRange, playerMask);

            if(hit) playerTransform = hit.transform;
            else
            {
                distanceToPlayer = float.PositiveInfinity;
                dirToPlayer = Vector2.zero;
                return;
            }
        }

        if(!playerTransform.gameObject.activeInHierarchy)
        {
            playerTransform = null;
            distanceToPlayer = float.PositiveInfinity;
            dirToPlayer = Vector2.zero;
            return;
        }

        Vector2 myPos = transform.position;
        Vector2 playerPos = playerTransform.position;

        Vector2 toPlayer = playerPos - myPos;
        distanceOfX = toPlayer.x;
        distanceToPlayer = toPlayer.magnitude;
        dirToPlayer = (distanceToPlayer > 0.0001f) ? toPlayer / distanceToPlayer : Vector2.zero;

        if(state == Felmos_State.Aggro || state == Felmos_State.Attack)
        {
            if(Mathf.Abs(distanceOfX) > freezeZoneX)
            {
                facingX = distanceOfX > 0f ? -1 : 1;
            }
        }
    }

    void RunFSM()
    {
        switch (state)
        {
            case Felmos_State.Idle: TickIdle(); offGuardTimer -= Time.deltaTime; break;
            case Felmos_State.Patrol: TickPatrol(); offGuardTimer -= Time.deltaTime; break;
            case Felmos_State.Alerted: break;
            case Felmos_State.Aggro: TickAggro(); offGuardTimer = alertedStateDuration; break;
            case Felmos_State.TakeDamage : TickTakeDamage(); break;
            case Felmos_State.Dead: break;
        }
    }

    void TickIdle()
    {
        StopXY();

        if (!enablePatrol && !enableAggro) return;

        if(!canMove) return;

        if (enableAggro && distanceToPlayer <= aggroRange && !isHit)
        {
            if(offGuardTimer <= 0 && state == Felmos_State.Idle)
            {
                ChangeState(Felmos_State.Alerted);
                StartCoroutine(AlertRoutine());
                return;
            }
            else
            {
                ChangeState(Felmos_State.Aggro);
                animator?.SetTrigger("Aggro");
                sfx.Play("Felmos_Aggro");
                return;
            }
        }

        if (enablePatrol && stateTimer >= idleTime && !isHit) 
        {
            ApplyFlip();
            facingX *= -1;
            animator?.SetTrigger("Patrol");
            ChangeState(Felmos_State.Patrol);
            return;
        }
    }

    void TickPatrol()
    {
        if (enableAggro && distanceToPlayer <= aggroRange)
        {
            if(offGuardTimer <= 0 && state == Felmos_State.Patrol)
            {
                ChangeState(Felmos_State.Alerted);
                animator?.SetTrigger("Idle");
                StartCoroutine(AlertRoutine());
                return;
            }
            else
            {
                ChangeState(Felmos_State.Aggro);
                animator?.SetTrigger("Aggro");
                sfx.Play("Felmos_Aggro");
                return;               
            }
        }

        if (stateTimer >= patrolTime)
        {
            patrolDirIndex = (patrolDirIndex + 1) % patrolDirs.Length;

            animator?.SetTrigger("Idle");
            ChangeState(Felmos_State.Idle);
        }
        
        Vector2 dir = patrolDirs[patrolDirIndex];
        dir = ApplyGroundClampDown(dir);

        MoveXY(dir, patrolSpeed);       
    }

    IEnumerator AlertRoutine()
    {
        StopXY();
        alertSprite.SetActive(true);
        if(Mathf.Abs(distanceOfX) > freezeZoneX)
        {
            facingX = distanceOfX > 0f ? -1 : 1;
        }

        yield return new WaitForSeconds(1f);

        offGuardTimer = alertedStateDuration;
        alertSprite.SetActive(false);
        ChangeState(Felmos_State.Aggro);
        animator?.SetTrigger("Aggro");
        sfx.Play("Felmos_Aggro");
    }

    void TickAggro()
    {
        if(Mathf.Abs(distanceOfX) > freezeZoneX) ApplyFlip();

        if (distanceToPlayer >= aggroRange)
        { 
            ChangeState(Felmos_State.Idle);
            animator?.SetTrigger("Idle"); 
            offGuardTimer = alertedStateDuration;
            return;
        }

        if(isRetreating)
        {
            StopXY();
            return;
        }

        if (enableKeepDistance &&
        retreatCooldownTimer <= 0f &&
        distanceToPlayer < retreatStartDistance &&
        retreatRoutine == null &&
        !isAttacking && !isHit)
        {
            retreatRoutine = StartCoroutine(KeepDistanceRoutine());
            return;
        }

        Vector2 desired = dirToPlayer;
        
        desired = ApplyGroundClampDown(desired);

        MoveXY(desired, aggroSpeed);

        if (enableShoot && isAttackReady && distanceToPlayer <= attackRange && !isAttacking && !isHit)
        {
            StartAttack();
            return;
        }
    }

    IEnumerator KeepDistanceRoutine()
    {
        isRetreating = true;

        StopXY();
        yield return new WaitForSeconds(waitBeforeRetreat);

        float t = 0f;
        while (t < retreatDuration)
        {
        
            if (isDead) break;
            if (state != Felmos_State.Aggro) break;
            if (!playerTransform) break;

        
            Vector2 retreatDir = -dirToPlayer;

            retreatDir = ApplyGroundClampDown(retreatDir);

            if (retreatDir.sqrMagnitude < 0.0001f)
                StopXY();
            else
                MoveXY(retreatDir.normalized, retreatSpeed);

            t += Time.deltaTime;
            yield return null;
        }

        StopXY();

        retreatCooldownTimer = retreatCooldown;

        isRetreating = false;
        retreatRoutine = null;
    }

    Vector2 ApplyGroundClampDown(Vector2 desired)
    {
        if (!enableMinHeightLimit) return desired;
        if (state != Felmos_State.Aggro) return desired; 

        Vector2 origin = groundRayOrigin ? (Vector2)groundRayOrigin.position : (Vector2)transform.position;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckRayLength, groundMask);

        Debug.DrawRay(origin, Vector2.down * groundCheckRayLength, hit.collider ? Color.red : Color.green);

    
        if (hit.collider != null && desired.y < 0f)
        {
            desired.y = 0f;

            if (desired.sqrMagnitude > 0.0001f)
                desired.Normalize();
        }

        return desired;
    }

    #region Attack
    void StartAttack()
    {
        if(runningRoutine != null) StopCoroutine(runningRoutine);
        isAttacking = true;
        isAttackReady = false;
        canMove = false;
        animator?.SetTrigger("ReadyAttack");
        runningRoutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        ChangeState(Felmos_State.Attack);

        StopXY();

        yield return new WaitForSeconds(readyAttackWindup);

        sfx.Play("Felmos_Attack");
        animator?.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        ChangeState(Felmos_State.Idle);
        animator?.SetTrigger("Idle");

        StartCoroutine(AttackCoolDown());

        yield return new WaitForSeconds(standByTime);

        canMove = true;
    }

    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(attackCoolTime);
        isAttackReady = true;
    }

    void ShootProjectile()
    {
        if (bulletPrefab && firePos && playerTransform)
        {
            var go = Instantiate(bulletPrefab, firePos.position, Quaternion.identity);
            Vector2 dir = (Vector2)(playerTransform.position - firePos.position);

            var bullet = go.GetComponent<FelmosBullet>();
            if (bullet) bullet.Initialize(dir, bulletDamage);

            return;
        }
    }
    #endregion

    void ShowWarningVFX()
    {
        WarningVFX.SetActive(true);
        vfxAnimator.Play("MaligureWarningVFX", 0, 0f);
    }

    void HideWarningVFX()
    {
        if (!WarningVFX) return;
        WarningVFX.SetActive(false);
    }

    void ChangeState(Felmos_State next)
    {
        state = next;
        stateTimer = 0f;

        if(state != Felmos_State.TakeDamage && isHit)
        {
            isHit = false;
        }
    }

    void MoveXY(Vector2 moveDir, float speed)
    {
        rb.linearVelocity = moveDir * speed;
    }

    void StopXY()
    {
        rb.linearVelocity = Vector2.zero;
    }

    void ApplyFlip()
    {
        transform.localScale = new Vector3(facingX >0 ? -originScale.x : originScale.x, originScale.y, originScale.z);
    }

    void TickTakeDamage()
    {
        if(isAttacking) return;

        if(stateTimer >= hitStunTime)
        {
            isHit = false;
            hitLockTimer = hitLockDuration;
            ChangeState(Felmos_State.Idle);
            animator?.SetTrigger("Idle");
        }
    }

    public void TakeDamage(float amount)
    {
        hp.TakeDamage(amount);
        sfx.Play("Felmos_OnDamage");

        if(isAttacking || isDead) return;

        OnHit(transform.position - Vector3.right);
    }

    void IDamageable.TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        hp.TakeDamage(amount);
        sfx.Play("Felmos_OnDamage");

        if(isAttacking || isDead) return;

        lastHitFrom = (Vector2)attackerWorldPosition;
        OnHit((Vector2)attackerWorldPosition);
    }


    public virtual void OnHit(Vector2 attackWorldPosition)
    {
        ChangeState(Felmos_State.TakeDamage);
        animator?.SetTrigger("Hit");
        isHit = true;

        Vector2 dir = ((Vector2)transform.position - attackWorldPosition).normalized;
        dir = new Vector2(dir.x * knockBackXForce, knockBackYForce);
        rb.linearVelocity = dir;
    }

    public virtual void OnDied()
    {
        if(isDead) return;

        isDead = true;
        rb.gravityScale = 9.81f;
        sfx.Play("Felmos_Death");

        ChangeState(Felmos_State.Dead);
        animator?.SetTrigger("Dead");

        StopAllCoroutines();
        monsterEvent.RaiseMonsterDead();
        isAttacking = false;

        GetComponent<Collider2D>().enabled = false;
        //rb.linearVelocity = Vector2.zero;
        //rb.bodyType = RigidbodyType2D.Kinematic;

        GameObject.Destroy(this.gameObject, 3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}


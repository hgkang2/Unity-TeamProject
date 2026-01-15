using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Malirgue : MonoBehaviour, IDamageable
{
    public bool enablePatrol = false;
    public bool enableAggro = false;
    public bool enableAttack = false;
    public bool enableSkill = false;

    public enum malirgue_State { Idle, Patrol, Aggro, Attack, Skill, TakeDamage, Dead }
    public malirgue_State state = malirgue_State.Idle;
    public float stateTimer;

    [Header("Refs")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] Animator vfxAnimator;
    [SerializeField] SpriteRenderer spriteRenderer;
    HP hp;
    [SerializeField] GameObject attackHitBox;
    [SerializeField] GameObject WarningVFX;
    
    [Header("Idle")]
    [SerializeField] float idleTime;

    [Header("Patrol")]
    [SerializeField] float patrolSpeed;
    [SerializeField] float patrolTime;

    [Header("Aggro")]
    [SerializeField] float aggroRange;
    [SerializeField] float aggroSpeed;
    [SerializeField] float freezeZoneX;
    [SerializeField] float offGuardTimer = 0f;
    [SerializeField] float alertedStateDuration = 10f;
    [SerializeField] GameObject alertSprite;

    int moveDirX = 1;

    [Header("Attack")]
    [SerializeField] float attackRange;
    [SerializeField] float readyAttackWindup;
    [SerializeField] float attackCoolTime;
    [SerializeField] float attackDuration;
    [SerializeField] float maxHeightDiffForAttack;
    [SerializeField] float delayTime;
    [SerializeField] float standByTime;
    bool isHeigtForAttackOk;
    bool isAttacking = false;
    bool isAttackReady = true;
    bool isActionLocked = false;

    [Header("Skill")]
    [SerializeField] float skillRange;
    [SerializeField] float readySkillWindup;
    [SerializeField] float skillCoolTime;
    [SerializeField] float skillDuration;
    bool isUsingSkill = false;
    bool isSkillReady = true;

    [Header("Detect Player")]
    [SerializeField] Transform playerTransform;
    [SerializeField] LayerMask playerMask;
    [SerializeField] float distanceToPlayer;
    float distanceOfX;

    int facingX = 1;
    Vector3 originScale;
    bool canMove = true;
    

    Coroutine runningRoutine;
    Coroutine lockActionRoutine;

    [Header("OnDamage")]
    [SerializeField] float knockBackXForce;
    [SerializeField] float knockBackYForce;
    Vector2 lastHitFrom;
    bool isHit = false;

    bool isDead = false;

    private void Awake() {
        if(!rb) rb = GetComponent<Rigidbody2D>();
        if(!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if(!animator) animator = GetComponent<Animator>();
        if (!hp) hp = GetComponent<HP>();
        if(!vfxAnimator) vfxAnimator = GetComponent<Animator>();

        isDead = false;

        hp.OnDied += OnDied;

        originScale = transform.localScale;
        facingX = 1;
        moveDirX = 1;

        ChangeState(malirgue_State.Idle);
    }

    public void OnDestroy()
    {
        hp.OnDied -= OnDied;
    }

    private void Update() {
        PlayerDetect();

        stateTimer += Time.deltaTime;
        MonsterGroundCheck();
        RunFSM();

        ApplyFlip();   
        hitLockTimer -= Time.deltaTime;
        offGuardTimer -= Time.deltaTime;
    }

    void ChangeState(malirgue_State next)
    {
        state = next;
        stateTimer = 0f;

        if(state != malirgue_State.TakeDamage && isHit)
        {
            isHit = false;
        }
    }

    void RunFSM()
    {
        switch(state)
        {
            case malirgue_State.Idle: TickIdle(); break;
            case malirgue_State.Patrol: TickPatrol(); break;
            case malirgue_State.Aggro: TickAggro(); break;
            case malirgue_State.Attack: if(cliffStopped) StopX(); break;
            case malirgue_State.Skill: if(cliffStopped) StopX(); break;
            case malirgue_State.TakeDamage: TickTakeDamage(); break;   
            case malirgue_State.Dead: break;
        }
    }

    void PlayerDetect()
    {
        if(!playerTransform)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, aggroRange, playerMask);

            if(hit)
            {
                playerTransform = hit.transform;
            } 
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

        if (state == malirgue_State.Aggro)
        {
            if (Mathf.Abs(distanceOfX) > freezeZoneX)
            {
                moveDirX = distanceOfX > 0f ? 1 : -1;
            }
        }
    }

    void TickIdle()
    {
        StopX();

        if(!enablePatrol && !enableAggro) return;

        if(!canMove) return;

        if(enablePatrol && stateTimer >= idleTime && !isHit)
        {
            ApplyFlip();
            
            moveDirX *= -1;
            ChangeState(malirgue_State.Patrol);
            animator?.SetTrigger("Patrol");
            return;
        }

        if(enableAggro && distanceToPlayer <= aggroRange && !isHit)
        {
            if(offGuardTimer <= 0)
            {
                StartCoroutine(AlertRoutine());
                return;
            }
            else
            {
                ChangeState(malirgue_State.Aggro);
                animator?.SetTrigger("Aggro");
                return;
            }
        }
    }

    void TickPatrol()
    {
        if(stateTimer >= patrolTime)
        {
            StopX();
            ChangeState(malirgue_State.Idle);
            animator?.SetTrigger("Idle");
            return;
        }

        if(enableAggro && distanceToPlayer <= aggroRange)
        {
            if(offGuardTimer <= 0)
            {
                StopX();
                StartCoroutine(AlertRoutine());
                return;
            }
            else
            {
                StopX();
                ChangeState(malirgue_State.Aggro);
                animator?.SetTrigger("Aggro");
                return;
            } 
        }

        facingX = moveDirX;

        if(cliffStopped)
        {
            StopX();
            return;
        }

        MoveX(moveDirX, patrolSpeed);
    }

    IEnumerator AlertRoutine()
    {
        StopX();
        if (Mathf.Abs(distanceOfX) > freezeZoneX)
            facingX = distanceOfX > 0f ? 1 : -1;
        animator?.SetTrigger("Idle");
        alertSprite.SetActive(true);

        yield return new WaitForSeconds(1f);

        alertSprite.SetActive(false);
        ChangeState(malirgue_State.Aggro);
        animator?.SetTrigger("Aggro");
    }

    void TickAggro()
    {
        float dy = playerTransform ? Mathf.Abs(playerTransform.position.y - transform.position.y) : float.PositiveInfinity;

        isHeigtForAttackOk = dy <= maxHeightDiffForAttack;

        if(!isAttacking)
        {
            float deadZone = 0.05f;
            if (Mathf.Abs(distanceOfX) > deadZone) facingX = moveDirX;
        }

        if(distanceToPlayer >= aggroRange)
        {
            ChangeState(malirgue_State.Idle);
            animator?.SetTrigger("Idle");
            offGuardTimer = alertedStateDuration;
            return;
        }

        if(cliffStopped)
        {
            StopX();
            return;
        }

        MoveX(moveDirX, aggroSpeed);        

        if(isHit && isHeigtForAttackOk) return;

        if (enableSkill && distanceToPlayer <= skillRange &&
            distanceToPlayer >= attackRange &&
            isSkillReady && !isUsingSkill)
        {
            StartSkill();
            return;
        }

        if(enableAttack && distanceToPlayer <= attackRange && isAttackReady && !isAttacking)
        {
            StartAttack();
            return;
        }
    }

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
        ChangeState(malirgue_State.Attack);
        StopX();
        yield return new WaitForSeconds(readyAttackWindup);

        animator?.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        ChangeState(malirgue_State.Idle);
        animator?.SetTrigger("Idle");
        HideWarningVFX();
        
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
        animator?.SetTrigger("ReadyAttack");
        runningRoutine = StartCoroutine(SkillRoutine());
    }

    [SerializeField] float skillDashForce;
    IEnumerator SkillRoutine()
    {
        ChangeState(malirgue_State.Skill);
        StopX();
        animator?.SetTrigger("ReadySkill");

        yield return new WaitForSeconds(readySkillWindup);

        //sfx.Play("AttackSound");
        animator?.SetTrigger("Skill");
        //rb.AddForce(10f * Vector2.right * facingX, ForceMode2D.Impulse);
        rb.linearVelocity = new Vector2(skillDashForce * facingX, rb.linearVelocity.y);

        yield return new WaitForSeconds(skillDuration);

        StopX();
        isUsingSkill = false;
        ChangeState(malirgue_State.Idle);
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

    void ApplyFlip()
    {
        transform.localScale = new Vector3(facingX > 0 ? originScale.x : -originScale.x, originScale.y, originScale.z);
    }

    void MoveX(int dirX, float speed)
    {
        rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
    }

    void StopX()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    [SerializeField] float hitStunTime;
    [SerializeField] float hitLockDuration;
    float hitLockTimer = 0f;
    void TickTakeDamage()
    {
        if(isAttacking) return;

        if(stateTimer >= hitStunTime)
        {
            isHit = false;
            hitLockTimer = hitLockDuration;
            ChangeState(malirgue_State.Idle);
            animator?.SetTrigger("Idle");
        }
    }

    public void TakeDamage(float amount)
    {
        hp.TakeDamage(amount);

        if(isAttacking || isDead) return;

        OnHit(transform.position - Vector3.right);
    }

    void IDamageable.TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        hp.TakeDamage(amount);

        if (isAttacking || isDead) return;

        lastHitFrom = (Vector2)attackerWorldPosition;
        OnHit((Vector2)attackerWorldPosition);
    }

    
    public virtual void OnHit(Vector2 attackWorldPosition)
    {
        ChangeState(malirgue_State.TakeDamage);
        animator?.SetTrigger("Hit");
        isHit = true;

        Vector2 dir = ((Vector2)transform.position - attackWorldPosition).normalized;
        dir = new Vector2(dir.x * knockBackXForce, knockBackYForce);
        rb.linearVelocity = dir;
    }

    public virtual void OnDied()
    {
        if (isDead) return;

        StopX();
        isDead = true;
        HideWarningVFX();

        ChangeState(malirgue_State.Dead);
        animator?.SetTrigger("Dead");

        StopAllCoroutines();
        isAttacking = false;

        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        //FindFirstObjectByType<Player>().Exp.AddExp(10);

        GameObject.Destroy(this.gameObject, 3f);
    }

    void HitboxOn()
    {
        attackHitBox.SetActive(true);
    }

    void HitBoxOff()
    {
        attackHitBox.SetActive(false);
    }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; 
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.blue; 
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }

    bool cliffStopped = false;
    bool isCliffAhead;
    [SerializeField] float rayLength;
    [SerializeField] float forwardPadding;
    [SerializeField] float bottomPadding;
    [SerializeField] LayerMask groundMask;
    [SerializeField] BoxCollider2D boxCollider;

    void MonsterGroundCheck()
    {
        if(state == malirgue_State.Dead) return;

        Bounds b = boxCollider.bounds;

        float dirX;

        // lastHitFrom 기반으로 밀려나는 방향 쪽으로 레이 시작점 이동
        if (state == malirgue_State.TakeDamage)
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

        if(state == malirgue_State.TakeDamage) return;
        animator?.SetBool("Aggro(IdleAni)", cliffStopped);
    }
}

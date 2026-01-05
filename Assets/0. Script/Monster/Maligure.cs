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

    [Header("Detect Player")]
    [SerializeField] Transform playerTransform;
    [SerializeField] LayerMask playerMask;
    [SerializeField] float distanceToPlayer;
    float distanceOfX;

    int facingX = 1;
    Vector3 originScale;
    bool canMove = true;
    

    Coroutine runningRoutine;

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
        RunFSM();

        ApplyFlip();   
        hitLockTimer -= Time.deltaTime;
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
            ChangeState(malirgue_State.Aggro);
            animator?.SetTrigger("Aggro");
            return;
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
            StopX();
            ChangeState(malirgue_State.Aggro);
            animator?.SetTrigger("Aggro");
            return;
        }

        MoveX(moveDirX, patrolSpeed);
        facingX = moveDirX;
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
            return;
        }

        MoveX(moveDirX, aggroSpeed);        

        if(isHit && isHeigtForAttackOk) return;

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
        StartCoroutine(AttackCoolDown());
        
        yield return new WaitForSeconds(standByTime);
        canMove = true;
    }

    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(attackCoolTime);
        isAttackReady = true;
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
    }
}

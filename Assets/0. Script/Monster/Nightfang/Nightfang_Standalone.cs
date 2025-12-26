using System.Collections;
using UnityEngine;

public class NightfangStandalone : MonoBehaviour, IDamageable
{
    HP hp;

    public enum State { Idle, Patrol, Aggro, Attack, Skill, TakeDamage, Dead }

    // === Animator Trigger Names ===
    const string TR_IDLE   = "Idle";
    const string TR_PATROL = "Patrol";
    const string TR_AGGRO  = "Aggro";
    const string TR_ATTACK = "Attack";
    const string TR_SKILL  = "ReadySkill";
    const string TR_HIT    = "Hit";
    const string TR_DEAD   = "Dead";

    [Header("Debug Step Test (�ܰ躰�� �ѱ�)")]
    public bool enablePatrol = false;
    public bool enableAggro = false;
    public bool enableAttack = false;
    public bool enableSkill = false;

    [Header("Refs")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] public SpriteRenderer spriteRenderer;

    [Header("Optional")]
    [SerializeField] public GameObject skillHitBoxObj;   // ���� ��Ʈ�ڽ�(������ �Ѱ�/����)
    [SerializeField] public GameObject attackHitboxObj;

    [Header("Detect")]
    [SerializeField] Transform player;      // ���� �ڵ� Ž��
    public string playerTag = "Player";
    public LayerMask playerMask;
    public float deadZoneX = 0.1f;

    [Header("Stats")]
    public float idleTime = 1.0f;

    public float patrolSpeed = 2.0f;
    public float patrolTime = 2.0f;

    public float aggroRange = 6.0f;
    public float aggroSpeed = 3.5f;

    public float maxHeightDiffForAttack = 0.8f;

    public float attackRange = 1.2f;
    public float readyAttackWindup = 1f;
    public float attackCoolTime = 1.0f;

    public float skillActiveRange = 3.5f;
    public float skillCoolTime = 0.2f;     //
    public float skillDuration;
    public float readySkillWindup = 0.25f;
    public float hitStunTime = 0.25f;



    [Header("Runtime")]
    public State state = State.Idle;
    public float stateTimer;

    // detector ���(���� MonsterDetector ����) :contentReference[oaicite:10]{index=10}
    public float distance;
    public float dx;
    public int moveDirX = 1;

    // facing/flip (���� Nightfang.Update�� flip ����) :contentReference[oaicite:11]{index=11}
    int facingX = 1;
    Vector3 originScale;

    // flags (���� MonsterBase flags) :contentReference[oaicite:12]{index=12}
    bool isAttack;
    bool isAttackReady = true;
    bool isUsingSkill;
    bool isSkillReady = true;

    int patrolDirX = 1;
    bool isDead;
    public bool isHit;

    Coroutine runningRoutine;

    Vector2 lastHitFrom;

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

        isDead = false;

        hp.OnDied += OnDied;

        originScale = transform.localScale;
        facingX = 1;
        patrolDirX = 1;

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

        if (Input.GetKeyDown(KeyCode.F1)) TakeDamage(10f);
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

        if (Mathf.Abs(dx) > deadZoneX)
            moveDirX = dx > 0f ? 1 : -1;
    }

    void RunFSM()
    {
        switch (state)
        {
            case State.Idle: TickIdle(); break;
            case State.Patrol: TickPatrol(); break;
            case State.Aggro: TickAggro(); break;
            case State.Attack: break;
            case State.Skill: break;
            case State.TakeDamage : TickTakeDamage();
                break;
            case State.Dead: break;
        }
    }

    void TickIdle()
    {
        StopX();
        //animator.SetTrigger("Idle");

        if (!enablePatrol && !enableAggro) return;

        if (enableAggro && isAttackReady &&distance <= aggroRange && !isHit)
        {
            ChangeState(State.Aggro);
            animator?.SetTrigger("Aggro");
            return;
        }

        if (enablePatrol && stateTimer >= idleTime && !isHit)
        {
            patrolDirX *= -1;
            ApplyFlip();
            ChangeState(State.Patrol);
            animator?.SetTrigger("Patrol");
        }
    }

    void TickPatrol()
    {
        // if (isAttack || isUsingSkill) return;

        if (enableAggro && distance <= aggroRange)
        {
            StopX();
            ChangeState(State.Aggro);
            animator?.SetTrigger("Aggro");
            return;
        }

        if (stateTimer >= patrolTime)
        {
            StopX();
            ChangeState(State.Idle);
            animator?.SetTrigger("Idle");
        }

        MoveX(patrolDirX, patrolSpeed);
        facingX = patrolDirX;
    }

    void TickAggro()
    {
        //if(isAttack) return;

        float dy = player
        ? Mathf.Abs(player.position.y - transform.position.y)
        : float.PositiveInfinity;

        bool heightOk = dy <= maxHeightDiffForAttack;

        if (!isAttack && !isUsingSkill)
        {
            float deadZone = 0.05f;
            if (Mathf.Abs(dx) > deadZone) facingX = moveDirX;
        }

        float stopDeadZone = 0.1f;
        if (Mathf.Abs(dx) <= stopDeadZone)
        {
            StopX();
        }
        else
        {
            MoveX(moveDirX, aggroSpeed);
        }

        if (distance >= aggroRange * 1.2f)
        {
            ChangeState(State.Idle);
            animator?.SetTrigger("Idle");
            return;
        }

        if (enableSkill && heightOk &&
            distance <= skillActiveRange &&
            distance >= attackRange &&
            isSkillReady && !isUsingSkill && !isHit)
        {
            StartSkill();
            return;
        }


        if (enableAttack && heightOk &&
            distance <= attackRange &&
            isAttackReady && !isAttack && !isHit)
        {
            StartAttack(); 
            return;
        }
    }

    void StartAttack()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        isAttack = true;
        isAttackReady = false;
        animator?.SetTrigger("ReadySkill");
        runningRoutine = StartCoroutine(AttackRoutine());
    }

    [SerializeField] float attackDuration = 1f;
    IEnumerator AttackRoutine()
    {
        ChangeState(State.Attack);
        StopX();
        yield return new WaitForSeconds(readyAttackWindup);

        animator?.SetTrigger("Attack");
        rb.AddForce(7f * Vector2.right * facingX, ForceMode2D.Impulse);

        yield return new WaitForSeconds(attackDuration);

        isAttack = false;
        StopX();

        ChangeState(State.Idle);
        animator?.SetTrigger("Idle");
        StartCoroutine(AttackCoolDown());
    }

    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(attackCoolTime);
        isAttackReady = true;
    }

    #region Skill
    public void StartSkill()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        isUsingSkill = true;
        isSkillReady = false;
        animator?.SetTrigger("ReadySkill");
        runningRoutine = StartCoroutine(SkillRoutine());
    }

    IEnumerator SkillRoutine()
    {
        ChangeState(State.Skill);
        spriteRenderer.color = Color.red;
        StopX();

        yield return new WaitForSeconds(readySkillWindup);

        animator.SetTrigger("Skill");
        rb.AddForce(10f * Vector2.right * facingX, ForceMode2D.Impulse);

        yield return new WaitForSeconds(skillDuration);

        isUsingSkill = false;
        StopX();
        
        spriteRenderer.color = Color.white;
        ChangeState(State.Idle);
        animator?.SetTrigger("Idle");

        StartCoroutine(SkillCooldownRoutine());
    }

    IEnumerator SkillCooldownRoutine()
    {
        yield return new WaitForSeconds(skillCoolTime);
        isSkillReady = true;
    }
    #endregion

    public void ChangeState(State next)
    {
        if (isDead && next != State.Dead) return;

        Debug.Log($"STATE: {state} -> {next}");
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
            facingX < 0 ? originScale.x : -originScale.x,
            originScale.y,
            originScale.z
        );
    }

    void StopX()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void MoveX(int dirX, float speed)
    {
        rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, skillActiveRange);
    }

    void TickTakeDamage()
    {
        if(isAttack) return;

        if (stateTimer >= hitStunTime)
        {
            ChangeState(State.Idle);
            animator?.SetTrigger("Idle");
        }
    }

    public void TakeDamage(float amount)
    {
        hp.TakeDamage(amount);

        if (isAttack || isDead) return;

        OnHit(transform.position - Vector3.right);
    }

    void IDamageable.TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        hp.TakeDamage(amount);

        if (isAttack || isDead) return;

        lastHitFrom = (Vector2)attackerWorldPosition;
        OnHit((Vector2)attackerWorldPosition);
    }

    [SerializeField]float knockBackXForce = 5f;
    [SerializeField]float knockBackYForce = 1f;

    public virtual void OnHit(Vector2 attackerWorldPosition)
    {
        ChangeState(State.TakeDamage);
        animator?.SetTrigger("Hit");

        Vector2 dir = ((Vector2)transform.position - attackerWorldPosition).normalized;
        dir = new Vector2(dir.x * knockBackXForce, knockBackYForce);
        rb.linearVelocity = dir;
    }

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
}

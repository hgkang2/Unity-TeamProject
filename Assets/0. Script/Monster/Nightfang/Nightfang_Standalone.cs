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
    public float attackDelay = 1f;
    public float attackRate = 1.0f;     // ���� ��Ÿ��

    public float skillActiveRange = 3.5f;
    public float skillDelay = 0.2f;     // ��ų �� ������(���� ��)
    public float skillCoolTime = 2.0f;  // ��ų ��Ÿ��
    public float readySkillWindup = 0.25f;

    public float nextAttackDelay = 1f;

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

        //Debug.Log($"Animator obj = {animator.gameObject.name}");
        //Debug.Log($"Controller = {animator.runtimeAnimatorController?.name}");
    }

    public void OnDestroy()
    {
        hp.OnDied -= OnDied;
    }

    void Update()
    {
        Debug.Log(state);

        if (isDead) return;

        // 1) Ž�� ������Ʈ
        PlayerDetect();

        // 2) ���� �ӽ�
        stateTimer += Time.deltaTime;
        RunFSM();

        // 3) �ø��� ������ �ٶ󺸴� ���⡱ �������� ����
        ApplyFlip();

        if (Input.GetKeyDown(KeyCode.F1)) TakeDamage(1.0f);
        if (Input.GetKeyDown(KeyCode.F2)) ChangeState(State.Idle);
        if (Input.GetKeyDown(KeyCode.F3)) ChangeState(State.Patrol);
        if (Input.GetKeyDown(KeyCode.F4)) ChangeState(State.Aggro);
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

            // Attack/Skill�� ���ִϸ��̼� �̺�Ʈ���� AttackStart/AttackEnd ���� �� �ٿ��� �ǰ�,
            // ���⼭�� �ڷ�ƾ���� �ܼ�ȭ����.
            case State.Attack: break;
            case State.Skill: break;
            case State.TakeDamage : TickTakeDamage();
                break;
            case State.Dead: break;
        }
    }

    void TickIdle()
    {
        if (isAttack || isUsingSkill) return;

        StopX();

        // 1�ܰ� �׽�Ʈ: Idle�� ����
        if (!enablePatrol && !enableAggro) return;

        // ��׷� �켱(���ϸ� �ݴ��)
        if (enableAggro && distance <= aggroRange)
        {
            TriggerAlertThenAggro();
            return;
        }

        if (enablePatrol && stateTimer >= idleTime)
        {
            patrolDirX *= -1;
            ApplyFlip();
            ChangeState(State.Patrol);
        }
    }

    void TickPatrol()
    {
        if (isAttack || isUsingSkill) return;

        if (enableAggro && distance <= aggroRange)
        {
            StopX();
            TriggerAlertThenAggro();
            return;
        }

        if (stateTimer >= patrolTime)
        {
            StopX();

            ChangeState(State.Idle);
        }

        MoveX(patrolDirX, patrolSpeed);
        facingX = patrolDirX;
    }

    void TriggerAlertThenAggro()
    {
        if (player)
        {
            int dir = (player.position.x - transform.position.x) >= 0f ? 1 : -1;
            facingX = dir;
            ApplyFlip();
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        ChangeState(State.Aggro);
    }

    void TickAggro()
    {
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

        // ��׷� ����
        if (distance >= aggroRange * 1.2f)
        {
            ChangeState(State.Idle);
            return;
        }

        if (enableSkill && heightOk &&
            distance <= skillActiveRange &&
            distance >= attackRange &&
            isSkillReady && !isUsingSkill)
        {
            // animator?.SetTrigger("ReadySkill");
            isUsingSkill = true;
            isSkillReady = false;

            StartSkill(); // �ڷ�ƾ ��� ����
            return;
        }


        if (enableAttack && heightOk &&
            distance <= attackRange &&
            isAttackReady && !isAttack)
        {
            isAttack = true;
            isAttackReady = false;

            StartAttack(); 
            return;
        }
    }

    void StartAttack()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        runningRoutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        ChangeState(State.Attack);
        StopX();

        yield return new WaitForSeconds(attackDelay);

        rb.AddForce(5f * Vector2.right * facingX, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.5f);

        StopX();
        
        isAttack = false;
        //LockX(true);

        yield return new WaitForSeconds(attackRate);
        ChangeState(State.Aggro);

        //LockX(false);
        isAttackReady = true;
    }

    public void StartSkill()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        runningRoutine = StartCoroutine(SkillRoutine());
    }

    IEnumerator SkillRoutine()
    {
        ChangeState(State.Skill);

        spriteRenderer.color = Color.red;
        StopX();

        yield return new WaitForSeconds(readySkillWindup);

        rb.AddForce(10f * Vector2.right * facingX, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.5f);

        StopX();
        isUsingSkill = false;
        //LockX(true);
        spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(skillDelay);

        ChangeState(State.Aggro);
        //LockX(false);

        StartCoroutine(SkillCooldownRoutine());
    }

    IEnumerator SkillCooldownRoutine()
    {
        yield return new WaitForSeconds(skillCoolTime);
        isSkillReady = true;
    }

    void ResetAllStateTriggers()
    {
        if (!animator) return;
        animator.ResetTrigger(TR_IDLE);
        animator.ResetTrigger(TR_PATROL);
        animator.ResetTrigger(TR_AGGRO);
        animator.ResetTrigger(TR_ATTACK);
        animator.ResetTrigger(TR_SKILL);
        animator.ResetTrigger(TR_HIT);
        animator.ResetTrigger(TR_DEAD);
    }

void PlayStateAnim(State s)
{
    if (!animator) return;

    // 같은 상태로 반복 호출 방지
    // var cur = animator.GetCurrentAnimatorStateInfo(0);
    // if (cur.IsName(s.ToString())) return;

    ResetAllStateTriggers();

    switch (s)
    {
        case State.Idle:      animator.SetTrigger(TR_IDLE); break;
        case State.Patrol:    animator.SetTrigger(TR_PATROL); break;
        case State.Aggro:     animator.SetTrigger(TR_AGGRO); break;
        case State.Attack:    animator.SetTrigger(TR_ATTACK); break;
        case State.Skill:     animator.SetTrigger(TR_SKILL); break;
        case State.TakeDamage:animator.SetTrigger(TR_HIT); break;
        case State.Dead:      animator.SetTrigger(TR_DEAD); break;
    }
}

    public void ChangeState(State next)
    {
        if (isDead && next != State.Dead) return;

        stateTimer = 0f;
        //Debug.Log($"STATE: {state} -> {next}");
        state = next;

        PlayStateAnim(state);
    }

    void ApplyFlip()
    {
        // flip�� �׻� facingX ����(���� Nightfang ���) :contentReference[oaicite:20]{index=20}
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
        if (state != State.Patrol && state != State.Aggro) return;

        rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
    }

    // void LockX(bool locked)
    // {
    //     if (!rb) return;

    //     rb.constraints = locked
    //         ? RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation
    //         : RigidbodyConstraints2D.FreezeRotation;
    // }

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
        if(!isAttackReady || isUsingSkill) return;

        if (stateTimer >= hitStunTime)
        {
            ChangeState(State.Idle);
        }
    }

    public void TakeDamage(float amount)
    {
        hp.TakeDamage(amount);

        if (isDead) return;

        OnHit(transform.position - Vector3.right);
    }

    void IDamageable.TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        hp.TakeDamage(amount);

        if (isDead) return;

        lastHitFrom = (Vector2)attackerWorldPosition;
        OnHit((Vector2)attackerWorldPosition);
    }

    public virtual void OnHit(Vector2 attackerWorldPosition)
    {
        StopX();
        ChangeState(State.TakeDamage);

        Vector2 dir = ((Vector2)transform.position - attackerWorldPosition).normalized;
        dir = new Vector2(dir.x, 0).normalized;
        rb.AddForce(dir * 5f, ForceMode2D.Impulse);
    }

    public virtual void OnDied()
    {
        if (isDead) return;

        StopX();
        isDead = true;

        ChangeState(State.Dead);

        StopAllCoroutines();
        isUsingSkill = false;

        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        FindFirstObjectByType<Player>().Exp.AddExp(10);

        GameObject.Destroy(this.gameObject, 3f);
    }
}

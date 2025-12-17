using System.Collections;
using UnityEngine;

public class NightfangStandalone : MonoBehaviour
{
    public enum State { Idle, Patrol, Aggro, Attack, Skill, TakeDamage, Dead }

    [Header("Debug Step Test (단계별로 켜기)")]
    public bool enablePatrol = false;
    public bool enableAggro = false;
    public bool enableAttack = false;
    public bool enableSkill = false;

    [Header("Refs")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Optional")]
    [SerializeField] GameObject skillCol;   // 공격 히트박스(있으면 켜고/끄기)
    [SerializeField] Transform player;      // 비우면 자동 탐색

    [Header("Detect")]
    public LayerMask playerMask;
    public float deadZoneX = 0.1f;

    [Header("Stats")]
    public float idleTime = 1.0f;

    public float patrolSpeed = 2.0f;
    public float patrolTime = 2.0f;

    public float aggroRange = 6.0f;
    public float aggroSpeed = 3.5f;

    public float attackRange = 1.2f;
    public float attackRate = 1.0f;     // 공격 쿨타임

    public float skillActiveRange = 3.5f;
    public float skillDelay = 0.2f;     // 스킬 후 딜레이(복귀 전)
    public float skillCoolTime = 2.0f;  // 스킬 쿨타임
    public float readySkillWindup = 0.25f;

    [Header("Runtime")]
    public State state = State.Idle;
    public float stateTimer;

    // detector 결과(예전 MonsterDetector 역할) :contentReference[oaicite:10]{index=10}
    public float distance;
    public float dx;
    public int moveDirX = 1;

    // facing/flip (예전 Nightfang.Update의 flip 로직) :contentReference[oaicite:11]{index=11}
    int facingX = 1;
    Vector3 originScale;

    // flags (예전 MonsterBase flags) :contentReference[oaicite:12]{index=12}
    bool isAttack;
    bool isAttackReady = true;
    bool isUsingSkill;
    bool isSkillReady = true;

    int patrolDirX = 1;
    bool isDead;

    Coroutine runningRoutine;

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

        originScale = transform.localScale;
        facingX = 1;
        patrolDirX = 1;

        if (skillCol) skillCol.SetActive(false);

        ChangeState(State.Idle);

        Debug.Log($"Animator obj = {animator.gameObject.name}");
        Debug.Log($"Controller = {animator.runtimeAnimatorController?.name}");
    }

    void Update()
    {
        if (isDead) return;

        // 0) 플레이어 찾기 (MonsterDetector가 하던 FindFirstObjectByType<Player> 대체) :contentReference[oaicite:13]{index=13}
        if (!player)
        {
            //var p = FindFirstObjectByType<Player>();
            //if (p) player = p.transform;
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }

        // 1) 탐지 업데이트
        UpdateDetect();

        // 2) 상태 머신
        stateTimer += Time.deltaTime;
        RunFSM();

        // 3) 플립은 “현재 바라보는 방향” 기준으로 통일
        ApplyFlip();

        // (테스트 편의) 강제 상태 전환 키
        // F2: Idle, F3: Patrol, F4: Aggro
        if (Input.GetKeyDown(KeyCode.F2)) ChangeState(State.Idle);
        if (Input.GetKeyDown(KeyCode.F3)) ChangeState(State.Patrol);
        if (Input.GetKeyDown(KeyCode.F4)) ChangeState(State.Aggro);
    }

    void UpdateDetect()
    {
        if (!player)
        {
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

            // Attack/Skill은 “애니메이션 이벤트”로 AttackStart/AttackEnd 같은 걸 붙여도 되고,
            // 여기서는 코루틴으로 단순화했음.
            case State.Attack:
            case State.Skill:
            case State.TakeDamage:
            case State.Dead:
                break;
        }
    }

    void TickIdle()
    {
        StopX();

        // 1단계 테스트: Idle만 유지
        if (!enablePatrol && !enableAggro) return;

        // 어그로 우선(원하면 반대로)
        if (enableAggro && distance <= aggroRange)
        {
            TriggerAlertThenAggro();
            return;
        }

        if (enablePatrol && stateTimer >= idleTime)
        {
            animator?.SetTrigger("Patrol");
            ChangeState(State.Patrol);
        }
    }

    void TickPatrol()
    {
        MoveX(patrolDirX, patrolSpeed);
        facingX = patrolDirX;

        if (enableAggro && distance <= aggroRange)
        {
            TriggerAlertThenAggro();
            return;
        }

        if (stateTimer >= patrolTime)
        {
            animator?.SetTrigger("Idle");
            ChangeState(State.Idle);
        }
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

        animator.ResetTrigger("Aggro");
        animator.SetTrigger("Alert");

        ChangeState(State.Aggro);
    }

    void TickAggro()
    {
        if (animator && animator.GetCurrentAnimatorStateInfo(0).IsName("Alert"))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // Aggro에서 facing 결정(예전 Nightfang 로직) :contentReference[oaicite:14]{index=14}
        if (!isAttack && !isUsingSkill)
        {
            float deadZone = 0.05f;
            if (Mathf.Abs(dx) > deadZone) facingX = moveDirX;
        }

        // 이동(예전 MonsterBase.Aggro 추격/정지) :contentReference[oaicite:15]{index=15}
        float stopDeadZone = 0.1f;
        if (Mathf.Abs(dx) <= stopDeadZone) StopX();
        else MoveX(moveDirX, aggroSpeed);

        // 어그로 해제
        if (distance >= aggroRange * 1.2f)
        {
            animator?.SetTrigger("Idle");
            ChangeState(State.Idle);
            return;
        }

        // 스킬 조건(중거리)
        if (enableSkill &&
            distance <= skillActiveRange &&
            distance >= attackRange &&
            isSkillReady && !isUsingSkill)
        {
            animator?.SetTrigger("ReadySkill");
            isUsingSkill = true;
            isSkillReady = false;

            StartSkill(); // 코루틴 기반 실행
            return;
        }

        // 공격 조건(근거리)
        if (enableAttack &&
            distance <= attackRange &&
            isAttackReady && !isAttack)
        {
            animator?.SetTrigger("Attack");
            isAttack = true;
            isAttackReady = false;

            StartAttack(); // 코루틴 기반 실행
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

        // Attack() 핵심: 히트박스 on + 약한 대쉬 :contentReference[oaicite:16]{index=16}
        if (skillCol) skillCol.SetActive(true);
        rb.AddForce(2f * Vector2.right * facingX, ForceMode2D.Impulse);

        // “공격 판정 시간” (애니메이션 길이에 맞춰 조절)
        yield return new WaitForSeconds(0.15f);

        // OnAttackExit() 핵심: 히트박스 off + 쿨다운 :contentReference[oaicite:17]{index=17}
        if (skillCol) skillCol.SetActive(false);
        StopX();

        yield return new WaitForSeconds(attackRate);

        isAttack = false;
        isAttackReady = true;

        animator?.SetTrigger("Aggro");
        ChangeState(State.Aggro);
    }

    public void StartSkill()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        runningRoutine = StartCoroutine(SkillRoutine());
    }

    IEnumerator SkillRoutine()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        yield return new WaitForSeconds(readySkillWindup);

        ChangeState(State.Skill);

        // UseSkill() 핵심: 강한 대쉬 :contentReference[oaicite:18]{index=18}
        rb.AddForce(10f * Vector2.right * facingX, ForceMode2D.Impulse);

        // OnSkillExit() 핵심: 딜레이 후 Aggro 복귀 + 쿨타임 :contentReference[oaicite:19]{index=19}
        yield return new WaitForSeconds(skillDelay);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        isUsingSkill = false;

        animator?.SetTrigger("Aggro");
        ChangeState(State.Aggro);
        StartCoroutine(SkillCooldownRoutine());
    }

    IEnumerator SkillCooldownRoutine()
    {
        yield return new WaitForSeconds(skillCoolTime);
        isSkillReady = true;
    }

    void ChangeState(State next)
    {
        if (isDead && next != State.Dead) return;

        Debug.Log($"STATE: {state} -> {next}");
        state = next;
        stateTimer = 0f;
    }

    void ApplyFlip()
    {
        // flip은 항상 facingX 기준(원본 Nightfang 방식) :contentReference[oaicite:20]{index=20}
        transform.localScale = new Vector3(
            facingX < 0 ? originScale.x : -originScale.x,
            originScale.y,
            originScale.z
        );
    }

    void StopX()
    {
        if (!rb) return;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void MoveX(int dirX, float speed)
    {
        if (!rb || isUsingSkill || isAttack) return;
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
}

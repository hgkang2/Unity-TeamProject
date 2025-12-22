using System.Collections;
using UnityEngine;

public class Felmos_Test : MonoBehaviour
{
    public enum State { Idle, Patrol, Aggro, Dead }

    [Header("Test")]
    public bool enablePatrol = true;
    public bool enableAggro = true;
    public bool enableShoot = true;
    public bool enableKeepDistance = true;
    public bool enableMinHeightLimit = true;

    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;          
    [SerializeField] private Transform firePos;          

    [Header("Player Detect")]
    [SerializeField] private Transform player;           // 비우면 태그로 찾음
    public string playerTag = "Player";
    public float aggroRange = 8f;
    public float shootRange = 6f;

    [Header("Movement")]
    public float idleTime = 0.3f;

    public float patrolSpeed = 2f;
    public float patrolTime = 1.0f;

    public float aggroSpeed = 3.5f;

    [Header("Min Height Limit")]
    public float minHeight = 3f;         // y가 이 값보다 내려가면 위로만 이동시키거나 보정
    public float heightPushUpSpeed = 2f;

    [Header("Keep Distance")]
    public float keepDistance = 4f;      // 이 거리보다 가까우면 멀어지려 함(후퇴)
    public float keepDistanceForce = 1.0f; // 후퇴 강도

    [Header("Shoot")]
    public GameObject bulletPrefab;      // FelmosBullet 프리팹
    public float bulletDamage = 10f;
    public float shootCooldown = 1.5f;

    [Header("Runtime")]
    public State state = State.Idle;
    public float stateTimer;
    public float distanceToPlayer;
    public Vector2 dirToPlayer;

    private int patrolDirIndex = 0;
    private readonly Vector2[] patrolDirs = new Vector2[]
    {
        Vector2.left, Vector2.right, Vector2.up, Vector2.down
    };

    private bool shootReady = true;
    private bool isDead;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        // 공중몹 기본 세팅 (중력 영향 최소화)
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        ChangeState(State.Idle);
    }

    void Update()
    {
        if (isDead) return;

        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }

        UpdateDetect();

        stateTimer += Time.deltaTime;
        RunFSM();
    }

    void UpdateDetect()
    {
        if (!player)
        {
            distanceToPlayer = float.PositiveInfinity;
            dirToPlayer = Vector2.zero;
            return;
        }

        Vector2 myPos = transform.position;
        Vector2 pPos = player.position;

        Vector2 toP = pPos - myPos;
        distanceToPlayer = toP.magnitude;
        dirToPlayer = (distanceToPlayer > 0.0001f) ? toP / distanceToPlayer : Vector2.zero;
    }

    void RunFSM()
    {
        switch (state)
        {
            case State.Idle: TickIdle(); break;
            case State.Patrol: TickPatrol(); break;
            case State.Aggro: TickAggro(); break;
            case State.Dead: break;
        }
    }

    void TickIdle()
    {
        rb.linearVelocity = Vector2.zero;

        if (!enablePatrol && !enableAggro) return;

        if (enableAggro && distanceToPlayer <= aggroRange)
        {
            animator?.SetTrigger("Aggro");
            ChangeState(State.Aggro);
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
        // Patrol: left/right/up/down 반복 (기존 Felmos와 동일한 방향 배열 방식)
        Vector2 dir = patrolDirs[patrolDirIndex];

        // 최소 높이 제한(아래로 내려가려 할 때 제어)
        dir = ApplyMinHeightLimit(dir);

        rb.linearVelocity = dir * patrolSpeed;

        // Aggro 전환
        if (enableAggro && distanceToPlayer <= aggroRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator?.SetTrigger("Aggro");
            ChangeState(State.Aggro);
            return;
        }

        if (stateTimer >= patrolTime)
        {
            patrolDirIndex = (patrolDirIndex + 1) % patrolDirs.Length;
            animator?.SetTrigger("Idle");
            ChangeState(State.Idle);
        }
    }

    void TickAggro()
    {
        if (!player)
        {
            ChangeState(State.Idle);
            return;
        }

        // 1) 기본은 플레이어 쪽으로 접근
        Vector2 desired = dirToPlayer;

        // 2) 일정 거리 유지(너무 가까우면 반대로 밀기)
        if (enableKeepDistance && distanceToPlayer < keepDistance)
        {
            // 가까울수록 더 강하게 반대로
            float t = Mathf.InverseLerp(keepDistance, 0f, distanceToPlayer);
            Vector2 retreat = -dirToPlayer * (1f + t * keepDistanceForce);
            desired = (desired + retreat).normalized;
        }

        // 3) 최소 높이 제한
        desired = ApplyMinHeightLimit(desired);

        rb.linearVelocity = desired * aggroSpeed;

        // 4) 사격 (스킬 없음, 1발 발사 + 쿨타임)
        if (enableShoot && shootReady && distanceToPlayer <= shootRange)
        {
            StartCoroutine(ShootRoutine());
        }

        if (distanceToPlayer > aggroRange * 1.2f)
        { 
            animator?.SetTrigger("Idle"); 
            ChangeState(State.Idle); 
        }
    }

    Vector2 ApplyMinHeightLimit(Vector2 dir)
    {
        if (!enableMinHeightLimit) return dir;

        // 현재 y가 minHeight보다 낮으면, 아래로 가려는 입력을 상쇄하고 위로 살짝 밀어줌
        if (transform.position.y < minHeight)
        {
            if (dir.y < 0f) dir.y = 0f;
            // 완전히 멈추기보단 위로 보정
            dir += Vector2.up * (heightPushUpSpeed / Mathf.Max(0.01f, aggroSpeed));
            dir.Normalize();
        }
        return dir;
    }

    IEnumerator ShootRoutine()
    {
        shootReady = false;

        animator?.SetTrigger("Attack");

        if (bulletPrefab && firePos && player)
        {
            var go = Instantiate(bulletPrefab, firePos.position, Quaternion.identity);
            Vector2 dir = (Vector2)(player.position - firePos.position);

            // 기존 FelmosBullet의 Initialize(dir, damage)
            var bullet = go.GetComponent<FelmosBullet>();
            if (bullet) bullet.Initialize(dir, bulletDamage);
        }

        yield return new WaitForSeconds(shootCooldown);
        shootReady = true;
    }

    void ChangeState(State next)
    {
        state = next;
        stateTimer = 0f;
    }
}


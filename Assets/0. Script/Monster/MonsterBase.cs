using System.Collections.Generic;
using UnityEngine;

public enum MonsterStateType { Idle, Patrol, Aggro, Take_Damage, Dead }

public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    HP hp;
    public float Damage { get { return monsterStats.skillDamage; } }

    [Header("Refs")]
    public MonsterStats monsterStats;
    public MonsterDetector detector;
    public MonsterGroundMovement mover;

    public MonsterStateType currentState = MonsterStateType.Idle;

    public LayerMask PlayerLayermask;

    bool isDead;

    [Header("Skill Info")]
    public bool isUsingSkill = false;
    public bool isSkillReady = true;

    [Header("Attack Info")]
    public bool isAttack = false;
    public bool isAttackReady = true;

    protected float StateTimer;
    public int patrolDirX = 1;

    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public GameObject Alert;

    public Collider2D MonsterHitBox;
    public GameObject SkillCol;

    public Vector2 lastHitFrom;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = GetComponent<HP>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        MonsterHitBox = GetComponent<Collider2D>();
        detector = GetComponent<MonsterDetector>();
        mover = GetComponent<MonsterGroundMovement>();

        isUsingSkill = false;
        isDead = false;

        hp.OnDied += OnDied;
    }

    public void OnDestroy()
    {
        hp.OnDied -= OnDied;
    }

    public virtual void Update()
    {
        if(TimeManager.IsPaused) return;
        if(isDead) return;

        detector?.Detector(this);

        if (Input.GetKeyDown(KeyCode.F1))
        {
            TakeDamage(10f);
        }

        MonsterFSM();
    }

    public virtual void MonsterFSM()
    {
        StateTimer += Time.deltaTime;

        Debug.Log(currentState);

        switch (currentState)
        {
            case MonsterStateType.Idle: Idle(); break;
            case MonsterStateType.Patrol: Patrol();  break;
            case MonsterStateType.Aggro: Aggro(); break;
            case MonsterStateType.Take_Damage: TakeDamageState(); break;
        }
    }

    public virtual void ChangeState(MonsterStateType nextState)
    {
        if (isDead && nextState != MonsterStateType.Dead)
            return;

        currentState = nextState;
        StateTimer = 0f;   
    }

    public virtual void Idle()
    {
        mover.StopX();

        if (StateTimer >= monsterStats.idleTime)
        {
            animator.SetTrigger("Patrol");
            ChangeState(MonsterStateType.Patrol);
            return;
        }

        if (detector.distance <= monsterStats.aggroRange)
        {
            animator.SetTrigger("Alert");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public virtual void Patrol()
    {
        mover.MoveX(patrolDirX, monsterStats.patrolSpeed);

        if (StateTimer >= monsterStats.patrolTime)
        {
            animator.SetTrigger("Idle");
            ChangeState(MonsterStateType.Idle);
            return;
        }

        if(detector.distance <= monsterStats.aggroRange)
        {
            animator.SetTrigger("Alert");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public virtual void Aggro()
    {
        if (isUsingSkill || !isAttackReady) return;

        float stopDeadZone = 0.1f;

        if (detector != null && Mathf.Abs(detector.dx) <= stopDeadZone)
            mover.StopX();
        else
            mover.MoveX(detector != null ? detector.moveDirx : patrolDirX, monsterStats.aggroSpeed);

        if (detector.distance >= monsterStats.aggroRange * 1.2f)
        { 
            animator.SetTrigger("Idle"); 
            ChangeState(MonsterStateType.Idle);
            return;
        }

        if (detector.distance <= monsterStats.skillActiveRange && detector.distance >= monsterStats.attackRange && isSkillReady && !isUsingSkill)
        {
            animator.SetTrigger("ReadySkill");
            isUsingSkill = true;
            isSkillReady = false;
        }

        if (detector.distance <= monsterStats.attackRange && isAttackReady && !isAttack)
        {
            animator.SetTrigger("Attack");
            isAttack = true;
            isAttackReady = false;
        }
    }

    public virtual void TakeDamageState()
    {
        mover.StopX();

        if (StateTimer >= monsterStats.hitStunTime)
        {
            if (detector.distance <= monsterStats.aggroRange)
            {
                animator.SetTrigger("Aggro");
                ChangeState(MonsterStateType.Aggro);
            }
            else
            {
                animator.SetTrigger("Idle");
                ChangeState(MonsterStateType.Idle);
            }
        }
    }

    public virtual void Attack() { }
    public virtual void OnAttackUpdate() { }
    public virtual void OnAttackExit() { }

    public virtual void UseSkill() { }
    public virtual void OnSkillUpdate() { }
    public virtual void OnSkillExit() { }

    public void TakeDamage(float amount)
    {
        if (hp == null) return;

        hp.TakeDamage(amount);

        if (isDead) return;

        OnHit(transform.position - Vector3.right);
    }

    void IDamageable.TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        if (hp == null) return;

        hp.TakeDamage(amount);

        if (isDead) return;

        lastHitFrom = (Vector2)attackerWorldPosition;
        OnHit((Vector2)attackerWorldPosition);
    }

    public virtual void OnHit(Vector2 attackerWorldPosition)
    {
        ChangeState(MonsterStateType.Take_Damage);
        animator.SetTrigger("Hit");

        Vector2 dir = ((Vector2)transform.position - attackerWorldPosition).normalized;

        float knockback = monsterStats.knockbackPower;
        rb.linearVelocity = new Vector2(dir.x * knockback, rb.linearVelocity.y);
    }

    public virtual void OnDied()
    {
        if (isDead) return;

        mover.StopX();
        isDead = true;

        ChangeState(MonsterStateType.Dead);

        animator.SetTrigger("Dead");

        StopAllCoroutines();
        isUsingSkill = false;

        GetComponent<Collider2D>().enabled = false;

        FindFirstObjectByType<Player>().Exp.AddExp(monsterStats.exp);

        GameObject.Destroy(this.gameObject, 3f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterStats.aggroRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, monsterStats.skillActiveRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, monsterStats.attackRange);
    }
}
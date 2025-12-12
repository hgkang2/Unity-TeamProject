using UnityEngine;

public enum MonsterStateType { Idle, Patrol, Aggro, Take_Damage, Dead }

public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    HP hp;

    public float Damage { get { return monsterStats.skillDamage; } }

    [Header("Stats")]
    public MonsterStats monsterStats;

    public MonsterStateType currentState = MonsterStateType.Idle;

    public LayerMask PlayerLayermask;

    bool isDead;

    public bool isUsingSkill = false;
    public bool isSkillReady = true;

    protected float StateTimer;
    public float DistanceToPlayer; 

    public Transform PlayerPosition; 
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public GameObject Alert;
    public Collider2D MonsterHitBox;
    public GameObject SkillCol;

    public Vector2 direction;

    public Vector2 lastHitFrom;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = GetComponent<HP>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        MonsterHitBox = GetComponent<Collider2D>();

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

        if (Input.GetKeyDown(KeyCode.F1))
        {
            TakeDamage(10f);
        }

        MonsterFSM();

        MonsterMovement();
    }

    public virtual void MonsterFSM()
    {
        StateTimer += Time.deltaTime;

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
        if(StateTimer >= monsterStats.idleTime)
        {
            animator.SetTrigger("Patrol");
            ChangeState(MonsterStateType.Patrol);
            return;
        }

        if (DistanceToPlayer <= monsterStats.aggroRange)
        {
            animator.SetTrigger("Alert");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public virtual void Patrol()
    {
        if (StateTimer >= monsterStats.patrolTime)
        {
            animator.SetTrigger("Idle");
            ChangeState(MonsterStateType.Idle);
            return;
        }

        if(DistanceToPlayer <= monsterStats.aggroRange)
        {
            animator.SetTrigger("Alert");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public virtual void Aggro()
    {
        if (isUsingSkill) return;

        if (DistanceToPlayer >= monsterStats.aggroRange * 1.2f)
        { 
            animator.SetTrigger("Idle"); 
            ChangeState(MonsterStateType.Idle); 
        }

        if (DistanceToPlayer <= monsterStats.skillActiveRange && isSkillReady && !isUsingSkill)
        {
            animator.SetTrigger("ReadySkill");
            isUsingSkill = true;
            isSkillReady = false;
        }
    }

    public virtual void TakeDamageState()
    {
        if (StateTimer >= monsterStats.hitStunTime)
        {
            if (DistanceToPlayer <= monsterStats.aggroRange)
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

    public virtual void MonsterMovement()
    {
        if (currentState == MonsterStateType.Take_Damage ||
        currentState == MonsterStateType.Dead)
        {
            rb.linearVelocity = new Vector2(0, -9.81f);
            return;
        }

        if(currentState == MonsterStateType.Idle)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (currentState == MonsterStateType.Patrol)
        {
            rb.linearVelocity = new Vector2(direction.x * monsterStats.patrolSpeed, rb.linearVelocity.y);
            return;
        }

        if(currentState == MonsterStateType.Aggro && !isUsingSkill)
        {
            rb.linearVelocity = new Vector2(direction.x * monsterStats.aggroSpeed, rb.linearVelocity.y);
            return;
        }
    }

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

    void IDamageable.TakeDamage(float amount, Vector2? attackerWorldPosition)
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

        isDead = true;

        ChangeState(MonsterStateType.Dead);

        animator.SetTrigger("Dead");

        StopAllCoroutines();
        isUsingSkill = false;

        GetComponent<Collider2D>().enabled = false;

        FindFirstObjectByType<Player>().Exp.AddExp(monsterStats.exp);

        GameObject.Destroy(this.gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if(player != null)
            {
                player.TakeDamage(monsterStats.colideDamage);
            }
        }
    }
}
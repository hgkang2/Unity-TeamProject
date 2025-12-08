using UnityEngine;

public struct MonsterData  // 
{
    public int exp;
    public float IdleTime; // 

    public int MoveDirection;   // 
    public float PatrolTime;    // 
    public float PatrolSpeed;   // 

    public float AggroRange;    // 
    public float AggroSpeed;

    public float Skill_Damage;
    public float Collde_Damage;

    public float Skill_Delay;

    public float SkillA_ActiveRange;
    public float SkillB_ActiveRange;
    public float SkillC_ActiveRange;

    public float SkillA_coolTime;
    public float SkillB_coolTime;
    public float SkillC_coolTime;

    public float HitStunTime;    // �ǰ� �� ���� ���� �ð�
    public float KnockbackPower; // �˹� ����
}

public enum MonsterStateType { Idle, Patrol, Aggro, Take_Damage, Dead }

public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    HP hp;

    public float Damage { get { return monsterData.Skill_Damage; } }

    public MonsterData monsterData;
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

    private void Start()
    {
        MonsterDataSetting();
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

    private void FixedUpdate()
    {
        if(TimeManager.IsPaused) return;
        DetectPlayer();
    }

    public abstract void MonsterDataSetting();

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
        rb.linearVelocity = new Vector2(0, -9.81f);
    }

    public virtual void Idle()
    {
        if(StateTimer >= monsterData.IdleTime)
        {
            animator.SetTrigger("Patrol");
            ChangeState(MonsterStateType.Patrol);
            return;
        }

        if (DistanceToPlayer <= monsterData.AggroRange)
        {
            animator.SetTrigger("Alert");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public virtual void Patrol()
    {
        if (StateTimer >= monsterData.PatrolTime)
        {
            animator.SetTrigger("Idle");
            ChangeState(MonsterStateType.Idle);
            return;
        }

        if(DistanceToPlayer <= monsterData.AggroRange)
        {
            animator.SetTrigger("Alert");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public virtual void Aggro()
    {
        if (isUsingSkill) return;

        if (DistanceToPlayer >= monsterData.AggroRange * 1.2f)
        { 
            animator.SetTrigger("Idle"); 
            ChangeState(MonsterStateType.Idle); 
        }

        if (DistanceToPlayer <= monsterData.SkillA_ActiveRange && isSkillReady && !isUsingSkill)
        {
            animator.SetTrigger("ReadySkill");
            isUsingSkill = true;
            isSkillReady = false;
        }
    }

    public virtual void DetectPlayer()
    {
        Collider2D detectCollider = Physics2D.OverlapCircle(transform.position, monsterData.AggroRange, PlayerLayermask);

        if (detectCollider != null && detectCollider.CompareTag("Player") && !isUsingSkill)
        {
            PlayerPosition = detectCollider.transform;

            Vector2 playerPos = new Vector2(PlayerPosition.position.x, transform.position.y);
            Vector2 myPos = new Vector2(transform.position.x, transform.position.y);

            direction = (playerPos - myPos).normalized;

            DistanceToPlayer = Vector2.Distance(transform.position, PlayerPosition.position);
        }
        else
        {
            PlayerPosition = null;
            DistanceToPlayer = Mathf.Infinity;
        }
    }

    public virtual void TakeDamageState()
    {
        if (StateTimer >= monsterData.HitStunTime)
        {
            if (DistanceToPlayer <= monsterData.AggroRange)
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

        if (currentState == MonsterStateType.Patrol)
        {
            rb.linearVelocity = new Vector2(direction.x * monsterData.PatrolSpeed, rb.linearVelocity.y);
            return;
        }

        if(currentState == MonsterStateType.Aggro && !isUsingSkill)
        {
            rb.linearVelocity = new Vector2(direction.x * monsterData.AggroSpeed, rb.linearVelocity.y);
            return;
        }
    }

    public virtual void UseSkill() { }

    public virtual void OnSkillUpdate() { }

    public virtual void OnSkillExit() { }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterData.AggroRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, monsterData.SkillA_ActiveRange);
    }

    public void TakeDamage(float amount)
    {
        if (hp == null) return;

        hp.TakeDamage(amount);

        if (isDead) return;

        OnHit(transform.position - Vector3.right);
    }

    void IDamageable.TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        if (hp == null) return; 

        hp.TakeDamage(amount);

        if (isDead) return;

        lastHitFrom = attackerWorldPosition;
        OnHit(attackerWorldPosition);
    }

    public virtual void OnHit(Vector2 attackerWorldPosition)
    {
        ChangeState(MonsterStateType.Take_Damage);
        animator.SetTrigger("Hit");

        Vector2 dir = ((Vector2)transform.position - attackerWorldPosition).normalized;

        float knockback = monsterData.KnockbackPower;
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

        FindFirstObjectByType<Player>().Exp.AddExp(monsterData.exp);

        GameObject.Destroy(this.gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if(player != null)
            {
                player.TakeDamage(monsterData.Collde_Damage);
            }
        }
    }
}
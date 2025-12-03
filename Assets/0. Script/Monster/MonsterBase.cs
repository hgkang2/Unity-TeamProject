using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct MonsterData  // 
{
    public float IdleTime; // 

    public int MoveDirection;   // 
    public float PatrolTime;    // 
    public float PatrolSpeed;   // 

    public float AggroRange;    // 
    public float AggroSpeed;

    public float Skill_Damage;

    public float Skill_Delay;

    public float SkillA_ActiveRange;
    public float SkillB_ActiveRange;
    public float SkillC_ActiveRange;

    public float SkillA_coolTime;
    public float SkillB_coolTime;
    public float SkillC_coolTime;
}

public enum MonsterStateType { Idle, Patrol, Aggro, Take_Damage,Dead }

public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    HP hp;
    Player player;

    public float Damage { get { return monsterData.Skill_Damage; } }

    public MonsterData monsterData;
    public MonsterStateType currentState = MonsterStateType.Idle;

    public LayerMask PlayerLayermask;

    public bool isUsingSkill = false;
    public bool isSkillReady = true;

    protected float StateTimer;
    public float DistanceToPlayer; 

    public Transform PlayerPosition; 
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public GameObject Alert;

    public Vector2 direction;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = GetComponent<HP>();
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        isUsingSkill = false;

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
        MonsterFSM();

        MonsterMovement();

        if (Input.GetKeyDown(KeyCode.A) && name == "Grimlog")
        {
            TakeDamage(10f);
        }
    }

    private void FixedUpdate()
    {
        DetectPlayer();
    }

    public abstract void MonsterDataSetting();

    public virtual void MonsterFSM()
    {
        StateTimer += Time.deltaTime;

        Debug.Log($"{gameObject.name} ({GetInstanceID()}) State: {currentState}");

        switch (currentState)
        {
            case MonsterStateType.Idle: Idle(); break;
            case MonsterStateType.Patrol: Patrol();  break;
            case MonsterStateType.Aggro: Aggro(); break;
        }
    }

    public virtual void ChangeState(MonsterStateType nextState)
    {
        currentState = nextState;
        StateTimer = 0f;
        rb.linearVelocity = Vector2.zero;
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

    public virtual void MonsterMovement()
    {
        if(currentState == MonsterStateType.Patrol)
        {
            rb.linearVelocity = new Vector2(direction.x * monsterData.PatrolSpeed, rb.linearVelocity.y);
            return;
        }

        if(currentState == MonsterStateType.Aggro && !isUsingSkill)
        {
            rb.linearVelocity = new Vector2(direction.x * monsterData.AggroSpeed, rb.linearVelocity.y);
            return;
        }

        if(currentState == MonsterStateType.Take_Damage)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if(currentState == MonsterStateType.Dead)
        {
            rb.linearVelocity = Vector2.zero;
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

    public void TakeDamage(float amount){
        hp.TakeDamage(amount);
    }

    void IDamageable.TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        hp.TakeDamage(amount);
    }

    void OnDied()
    {
        Debug.Log("dead");

        ChangeState(MonsterStateType.Dead);

        StopAllCoroutines();
        isUsingSkill = false;
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        GameObject.Destroy(this.gameObject, 3f);
    }
}
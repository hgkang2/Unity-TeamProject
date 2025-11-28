using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct MonsterData  // �� ������
{
    public float IdleTime; // ��� �ð�

    public int MoveDirection;   // �̵� ���� ,-1 = �������� �̵� / 1 = ���������� �̵� 
    public float PatrolTime;    // ����(�̵�)�� �ð�
    public float PatrolSpeed;   // ����(�̵�) �ӵ�

    public float AggroRange;    // ��׷� ����

    public float SkillA_ActiveRange;
    public float SkillB_ActiveRange;
    public float SkillC_ActiveRange;

    public float Skill_Damage;

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

    [SerializeField]
    public LayerMask PlayerLayermask;

    public bool isUsingSkill = false;
    public bool isSkillReady = true;

    public float StateTimer;
    public float DistanceToPlayer; // 플레이 거리

    public Transform PlayerPosition; // �÷��̾� ���� ��ġ
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    public Vector2 direction;

    public Animator animator;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = GetComponent<HP>();
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        isUsingSkill = false;
    }

    private void Start()
    {
        MonsterDataSetting();
    }

    public virtual void Update()
    {
        MonsterFSM();
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
            animator.SetTrigger("Aggro");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public virtual void Patrol()
    {
        transform.position += new Vector3(direction.x * monsterData.PatrolSpeed * Time.deltaTime,0,0);

        if(StateTimer >= monsterData.PatrolTime)
        {
            animator.SetTrigger("Idle");
            ChangeState(MonsterStateType.Idle);
            return;
        }

        if(DistanceToPlayer <= monsterData.AggroRange)
        {
            animator.SetTrigger("Aggro");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public virtual void Aggro()
    {
        if (isUsingSkill) return;

        transform.position += (Vector3)(direction * monsterData.PatrolSpeed * Time.deltaTime);

        if (DistanceToPlayer >= monsterData.AggroRange * 1.2f)
        {
            animator.SetTrigger("Idle");
            ChangeState(MonsterStateType.Idle);
            return;
        }

        if (DistanceToPlayer <= monsterData.SkillA_ActiveRange && isSkillReady && !isUsingSkill)
        {
            animator.SetTrigger("Skill");
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

    public virtual void UseSkill() { }

    public virtual void OnSkillUpdate() { }

    public virtual void OnSkillExit() { }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {

        }
    }

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
}
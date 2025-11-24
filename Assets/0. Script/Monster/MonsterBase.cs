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
public enum MonsterSkillType { None, Skill_A, Skill_B, Skill_C }

public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    HP hp;
    Player player;

    public float Damage { get { return monsterData.Skill_Damage; } }

    public MonsterData monsterData;
    public MonsterStateType currentState = MonsterStateType.Idle;
    public MonsterSkillType selectedSkill = MonsterSkillType.None;

    [SerializeField]
    LayerMask PlayerLayermask;
    
    public bool isUsingSkill = false;

    public float StateTimer;
    public float DistanceToPlayer; // 플레이 거리

    public bool isSkillReady;

    public Transform PlayerPosition; // �÷��̾� ���� ��ġ
    public Rigidbody2D rb;
    Vector2 direction;

    protected Coroutine skillCoroutine;

    public Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = GetComponent<HP>();
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();

        isUsingSkill = false;
    }

    private void Start()
    {
        MonsterDataSetting();
    }

    private void Update()
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
    }

    public virtual void Patrol()
    {
        transform.rotation = Quaternion.Euler(0, monsterData.MoveDirection > 0 ? 180 : 0, 0);

        transform.position += new Vector3(monsterData.MoveDirection * monsterData.PatrolSpeed * Time.deltaTime, 0f, 0f);

        if(StateTimer >= monsterData.PatrolTime)
        { 
            monsterData.MoveDirection *= -1;
            animator.SetTrigger("Idle");
            ChangeState(MonsterStateType.Idle);
            return;
        }
    }

    public virtual void Aggro()
    {
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

            return;
        }
    }

    public void DetectPlayer()
    {
        Collider2D detectCollider = Physics2D.OverlapCircle(transform.position, monsterData.AggroRange, PlayerLayermask);

        if (detectCollider != null)
        {
            if (detectCollider.CompareTag("Player"))
            {
                PlayerPosition = detectCollider.transform;

                Vector2 playerPos = new Vector2(PlayerPosition.position.x, transform.position.y);
                Vector2 myPos = new Vector2(transform.position.x, transform.position.y);

                direction = (playerPos - myPos).normalized;

                DistanceToPlayer = Vector2.Distance(transform.position, PlayerPosition.position);

                animator.SetTrigger("Aggro");
                ChangeState(MonsterStateType.Aggro);
                return;
            }
        }
        else
        {  
            PlayerPosition = null;

            DistanceToPlayer = Mathf.Infinity;
        }
    }

    public virtual void UseSkill()
    {

    }

    public virtual void SkillA() { }
    public virtual void SkillB() { }
    public virtual void SkillC() { }


    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            monsterData.MoveDirection *= -1; // ���� �浹�ϸ� �ݴ� �������� �̵�
        }
    }

    void OnDrawGizmos()
    {
        // ��׷� ���� (����)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterData.AggroRange);

        // ��ų �ߵ� ���� (�Ķ�)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, monsterData.SkillA_ActiveRange);
    }
    public void TakeDamage(float amount){
        hp.TakeDamage(amount);
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct MonsterData  // 적 데이터
{
    public float IdleTime; // 대기 시간

    public int MoveDirection;   // 이동 방향 ,-1 = 왼쪽으로 이동 / 1 = 오른쪽으로 이동 
    public float PatrolTime;    // 순찰(이동)할 시간
    public float PatrolSpeed;   // 순찰(이동) 속도

    public float AggroRange;    // 어그로 범위

    public float SkillA_ActiveRange;
    public float SkillB_ActiveRange;
    public float SkillC_ActiveRange;

    public float Skill_Damage;

    public float SkillA_coolTime;
    public float SkillB_coolTime;
    public float SkillC_coolTime;
}

public enum MonsterStateType { Idle, Patrol, Aggro, Skill, Take_Damage,Dead }
public enum MonsterSkillType { None, Skill_A, Skill_B, Skill_C }

public abstract class MonsterBase : MonoBehaviour ,IAttackable
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
    public float DistanceToPlayer; // 플레이어와 적 사이의 거리

    public bool isSkillReady;

    public Transform PlayerPosition; // 플레이어 현재 위치
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
            case MonsterStateType.Skill: Skill(); break;
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

        if (DistanceToPlayer <= monsterData.SkillA_ActiveRange && isSkillReady)
        {
            animator.SetTrigger("Skill");
            ChangeState(MonsterStateType.Skill);
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

                if (isUsingSkill) return;
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

    protected virtual MonsterSkillType DecideSkillType()
    {
        return MonsterSkillType.None;
    }

    public virtual void Skill()
    {
        MonsterSkillType type = DecideSkillType();
        if(type != MonsterSkillType.None)
        {
            UseSkill(type);
        }
    }

    protected virtual void UseSkill(MonsterSkillType skillType)
    {
        if (!isSkillReady || skillType == MonsterSkillType.None) return;

        if (skillCoroutine != null) StopCoroutine(skillCoroutine);

        skillCoroutine = StartCoroutine(ReadySkill(skillType));
    }

    protected virtual IEnumerator ReadySkill(MonsterSkillType skillType)
    {
        isSkillReady = false;

        switch (skillType)
        {
            case MonsterSkillType.Skill_A: yield return SkillA(); break;
            case MonsterSkillType.Skill_B: yield return SkillB(); break;
            case MonsterSkillType.Skill_C: yield return SkillC(); break;
        }

        isSkillReady = true;
        selectedSkill = MonsterSkillType.None;
    }

    public virtual void OnSkillUpdate() { }

    public virtual void OnSkillExit() { }

    protected virtual IEnumerator SkillA() { yield break; }
    protected virtual IEnumerator SkillB() { yield break; }
    protected virtual IEnumerator SkillC() { yield break; }


    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            player.HP.TakeDamage(monsterData.Skill_Damage);
        }
        if(collision.gameObject.CompareTag("Wall"))
        {
            monsterData.MoveDirection *= -1; // 벽에 충돌하면 반대 방향으로 이동
        }
    }

    void OnDrawGizmos()
    {
        // 어그로 범위 (빨강)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterData.AggroRange);

        // 스킬 발동 범위 (파랑)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, monsterData.SkillA_ActiveRange);
    }
}
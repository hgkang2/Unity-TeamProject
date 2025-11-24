using UnityEngine;

public class Monster : MonoBehaviour, IEnemyStradegy
{
    public EnemyStateType currentState = EnemyStateType.Idle;
    HP hp;
    [SerializeField] float damage = 20f;
    public float Damage { get { return damage; } }

    [SerializeField]
    float idleTime = 2f;
    float patrolSpeed = 2f;
    float patrolTime = 3f;
    float StateTimer = 0f;
    int movedir = 1;

    [SerializeField]
    float AggroDis = 6f;
    float AttackDis;
    float SkillDis = 3f;
    float SkillPower = 10f;

    Vector2 PatrolDir;

    float DistanceToPlayer;
    public Transform PlayerTransform;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PatrolDir = Vector2.right;
        ChangeState(EnemyStateType.Idle);
    }

    void Update()
    {
        if (PlayerTransform != null)
        {
            DistanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        }


        StateTimer += Time.deltaTime;
        //Debug.Log(currentState);

        switch (currentState)
        {
            case EnemyStateType.Idle:
                Idle();
                break;

            case EnemyStateType.Patrol:
                Patrol();
                break;

            case EnemyStateType.Chase:
                Chase();
                break;

            case EnemyStateType.Skill_A:
                Skill();
                break;
        }

    }


    public void ChangeState(EnemyStateType nextState)
    {
        currentState = nextState;
        StateTimer = 0f;
    }

    public void Idle()
    {
        // 정지 -> 순찰(이동)
        if (StateTimer >= idleTime)
        {
            ChangeState(EnemyStateType.Patrol);
        }

        // 플레이어가 어그로 범위 내에 진입 할 경우 추격
        if (DistanceToPlayer <= AggroDis)
        {
            ChangeState(EnemyStateType.Chase);
        }
    }

    public void Patrol()
    {
        // 순찰( 주변 이동)
        transform.position += new Vector3(movedir * patrolSpeed * Time.deltaTime, 0f, 0f);

        if (StateTimer >= patrolTime)
        {
            movedir *= -1;
            ChangeState(EnemyStateType.Idle);
        }

        if (DistanceToPlayer <= AggroDis)
        {
            ChangeState(EnemyStateType.Chase);
        }
    }

    public void Chase()
    {
        if(PlayerTransform == null) return;

        if (DistanceToPlayer <= SkillDis)
        {
            ChangeState(EnemyStateType.Skill_A);
        }

        // 플레이어를 향해 이동
        Vector2 targetPos = new Vector2(PlayerTransform.position.x, transform.position.y);
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * patrolSpeed;
    }

    public void Attack()
    {

    }

    public void Skill()
    {
        Vector2 targetPos = new Vector2(PlayerTransform.position.x, transform.position.y);

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;

        rb.linearVelocity = dir * SkillPower;
    }

    public void Dead()
    {
        ChangeState(EnemyStateType.Dead);

        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ���� �浹 �� �ݴ� �������� �̵�
        if (collision.gameObject.CompareTag("Wall"))
        {
            movedir *= -1;
        }

        //// �÷��̾�� �浹 �� �÷��̾� ü�� ����
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    //TempGameManager.instance.AttackDmg(CollisionDMG);
        //}
    }
}

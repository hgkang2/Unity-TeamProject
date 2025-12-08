using System.Collections;
using UnityEngine;

public class Felmos : MonsterBase
{
    int patrolDirIndex = 0;
    Vector2[] patrolDir = new Vector2[]
    { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

    float retreatRange = 4f;
    [SerializeField] float MinHeight = 3f;
    [SerializeField] bool retreating;

    [SerializeField] GameObject FelmosBullet;

    public Transform PlayerPos;
    public Transform FirePos;

    public override void Awake()
    {
        base.Awake();

        PlayerPos = GameObject.Find("Player").transform;
        direction.x = 1;
    }

    public override void Update()
    {
        base.Update();

        DetectGround();

        if (direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    public override void MonsterDataSetting()
    {
        monsterData.IdleTime = 3f;
        monsterData.PatrolSpeed = 5f;
        monsterData.PatrolTime = 3f;

        monsterData.AggroRange = 12f;
        monsterData.AggroSpeed = 7f;

        monsterData.Skill_Damage = 100f;
        monsterData.Skill_Delay = 0.5f;

        monsterData.SkillA_ActiveRange = 8f;
        monsterData.SkillA_coolTime = 10f;

        monsterData.HitStunTime = 0.5f;
        monsterData.KnockbackPower = 5f;
    }

    public override void Patrol()
    {
        Vector2 dir = patrolDir[patrolDirIndex];    

        rb.linearVelocity = dir * monsterData.PatrolSpeed;

        if (StateTimer >= monsterData.PatrolTime)
        {
            patrolDirIndex = (patrolDirIndex + 1) % patrolDir.Length;

            animator.SetTrigger("Idle");
            ChangeState(MonsterStateType.Idle);
        }

        if(DistanceToPlayer <= monsterData.AggroRange)
        {
            animator.SetTrigger("Alert");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public override void Aggro()
    {
        if (isUsingSkill || retreating) return;

        rb.linearVelocity = direction * monsterData.AggroSpeed;

        if (DistanceToPlayer <= retreatRange)
        {
            StartCoroutine(Retreat());
        }
       
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

    //public override void DetectPlayer()
    //{
    //    Collider2D detectCollider = Physics2D.OverlapCircle(transform.position, monsterData.AggroRange, PlayerLayermask);

    //    if (detectCollider != null && detectCollider.CompareTag("Player") && !isUsingSkill)
    //    {
    //        PlayerPosition = detectCollider.transform;

    //        Vector2 playerPos = new Vector2(PlayerPosition.position.x, PlayerPosition.position.y);
    //        Vector2 myPos = new Vector2(transform.position.x, transform.position.y);

    //        direction = (playerPos - myPos).normalized;

    //        DistanceToPlayer = Vector2.Distance(transform.position, PlayerPosition.position);
    //    }
    //    else
    //    {
    //        PlayerPosition = null;

    //        DistanceToPlayer = Mathf.Infinity;
    //    }
    //}

    void DetectGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, MinHeight);

        Debug.DrawRay(transform.position, Vector2.down * MinHeight, Color.purple);

        if(hit.collider != null)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
        }
    }

    public override void UseSkill()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public override void OnSkillExit()
    {
        var ShootSkill = Instantiate(FelmosBullet, FirePos.position, Quaternion.identity);

        Vector2 dir = PlayerPos.position - FirePos.position;
        float damage = monsterData.Skill_Damage;

        ShootSkill.GetComponent<FelmosBullet>().Initialize(dir, damage);

        StartCoroutine(SkillCooldown());
    }

    IEnumerator SkillCooldown()
    {
        yield return new WaitForSeconds(monsterData.Skill_Delay);

        isUsingSkill = false;
        animator.SetTrigger("Aggro");
        ChangeState(MonsterStateType.Aggro);

        yield return new WaitForSeconds(monsterData.SkillA_coolTime);

        isSkillReady = true;
    }

    IEnumerator Retreat()
    {
        retreating = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(1f);

        rb.linearVelocity = -direction * monsterData.AggroSpeed;

        yield return new WaitForSeconds(0.5f);

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(2f);

        retreating = false;
    }

    public override void MonsterMovement()
    {
        // 이 몹은 Patrol/Aggro에서 직접 rb.linearVelocity 관리하니까 여기서는 아무것도 안 함
    }
}
using System.Collections;
using UnityEngine;

public class Felmos : MonsterBase
{
    int patrolDirIndex = 0;
    Vector2[] patrolDir = new Vector2[]
    { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

    float retreatRange = 4f;
    float MinHeight = 3f;

    [SerializeField]
    GameObject FelmosBullet;

    public Transform PlayerPos;
    public Transform FirePos;

    public override void Awake()
    {
        base.Awake();
        direction.x = 1;
    }

    public override void Update()
    {
        base.Update();

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

        monsterData.Skill_Damage = 1f;
        monsterData.Skill_Delay = 0.5f;

        monsterData.SkillA_ActiveRange = 8f;
        monsterData.SkillA_coolTime = 10f;
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
        if (isUsingSkill) return;

        if(DistanceToPlayer <= retreatRange * 1.2f)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = direction * monsterData.PatrolSpeed;
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

    public override void DetectPlayer()
    {
        Collider2D detectCollider = Physics2D.OverlapCircle(transform.position, monsterData.AggroRange, PlayerLayermask);

        if (detectCollider != null && detectCollider.CompareTag("Player") && !isUsingSkill)
        {
            PlayerPosition = detectCollider.transform;

            Vector2 playerPos = new Vector2(PlayerPosition.position.x, PlayerPosition.position.y);
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

    void DetectGround()
    {

    }

    public override void UseSkill()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public override void OnSkillExit()
    {
        var ShootSkill = Instantiate(FelmosBullet, FirePos.position, Quaternion.identity);

        Vector2 dir = PlayerPos.position - FirePos.position;

        ShootSkill.GetComponent<FelmosBullet>().Initialize(dir);

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
}
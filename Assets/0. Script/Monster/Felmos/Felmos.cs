using System.Collections;
using UnityEngine;

public class Felmos : MonsterBase
{
    int patrolDirIndex = 0;
    Vector2[] patrolDir = new Vector2[]
    { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

    [SerializeField]
    GameObject FelmosBullet;
    public Transform FirePos;

    public override void Awake()
    {
        base.Awake();
        direction.x = 1;
    }

    public override void Update()
    {
        base.Update();

        if (direction.x >= 0)
        {
            //spriteRenderer.flipX = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction.x < 0)
        {
            //spriteRenderer.flipX = true;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public override void MonsterDataSetting()
    {
        monsterData.IdleTime = 3f;
        monsterData.PatrolSpeed = 5f;
        monsterData.PatrolTime = 3f;

        monsterData.AggroRange = 10f;

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
            animator.SetTrigger("Aggro");
            ChangeState(MonsterStateType.Aggro);
        }
    }

    public override void Aggro()
    {
        if (isUsingSkill) return;

        rb.linearVelocity = direction * monsterData.PatrolSpeed;

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

    public override void UseSkill()
    {
        rb.linearVelocity = Vector2.zero;
        var ShootSkill = Instantiate(FelmosBullet);
        ShootSkill.transform.position = FirePos.position;
    }

    public override void OnSkillExit()
    {
        StartCoroutine(SkillCooldown());
    }

    IEnumerator SkillCooldown()
    {
        yield return new WaitForSeconds(0.5f);

        isUsingSkill = false;
        animator.SetTrigger("Aggro");
        ChangeState(MonsterStateType.Aggro);

        yield return new WaitForSeconds(3f);

        isSkillReady = true;
    }
}
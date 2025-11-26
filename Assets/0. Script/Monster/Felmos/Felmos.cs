using System.Collections;
using UnityEngine;

public class Felmos : MonsterBase
{
    public override void MonsterDataSetting()
    {
        monsterData.IdleTime = 3f;
        monsterData.PatrolSpeed = 5f;
        monsterData.PatrolTime = 3f;

        monsterData.AggroRange = 10f;

        monsterData.SkillA_ActiveRange = 10f;
        monsterData.SkillA_coolTime = 10f;
    }

    int patrolDirIndex = 0;
    Vector2[] patrolDir = new Vector2[]
    { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

    [SerializeField]
    GameObject FelmosCorrosive;
    Transform FirePos;

    public override void Patrol()
    {
        transform.rotation = Quaternion.Euler(0, patrolDirIndex > 0 ? 180 : 0, 0);

        Vector2 dir = patrolDir[patrolDirIndex];
        transform.position += (Vector3)(dir * monsterData.PatrolSpeed * Time.deltaTime);

        if(StateTimer >= monsterData.PatrolTime)
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

    public override void UseSkill()
    {
        
    }
}

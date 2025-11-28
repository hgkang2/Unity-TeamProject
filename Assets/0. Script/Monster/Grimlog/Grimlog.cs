using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Grimlog : MonsterBase
{
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
            spriteRenderer.flipX = true;
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    public override void MonsterDataSetting()
    {
        monsterData.IdleTime = 3f;
        monsterData.PatrolSpeed = 3f;
        monsterData.PatrolTime = 3f;

        monsterData.MoveDirection = -1;

        monsterData.AggroRange = 10f;

        monsterData.SkillA_ActiveRange = 6f;

        monsterData.SkillA_coolTime = 15f;
    }

    public override void UseSkill()
    {
        rb.AddForce(10f * direction, ForceMode2D.Impulse);
    }

    public override void OnSkillExit()
    {
        StartCoroutine(SkillCooldown());
    }

    IEnumerator SkillCooldown()
    {
        
        yield return new WaitForSeconds(3f);
        
        isUsingSkill = false;
        animator.SetTrigger("Aggro");
        ChangeState(MonsterStateType.Aggro);
        

        yield return new WaitForSeconds(5f);

        isSkillReady = true;
        
    }
}

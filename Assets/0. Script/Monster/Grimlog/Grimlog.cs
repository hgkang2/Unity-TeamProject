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

        monsterData.AggroRange = 13f;
        monsterData.AggroSpeed = 5f;

        monsterData.Skill_Damage = 200f;
        monsterData.Skill_Delay = 3f;

        monsterData.SkillA_ActiveRange = 6f;
        monsterData.SkillA_coolTime = 12f;

        monsterData.HitStunTime = 0.5f;
        monsterData.KnockbackPower = 5f;
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
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        yield return new WaitForSeconds(monsterData.Skill_Delay);
        
        isUsingSkill = false;
        animator.SetTrigger("Aggro");
        ChangeState(MonsterStateType.Aggro);

        yield return new WaitForSeconds(monsterData.SkillA_coolTime);

        isSkillReady = true;
    }
}

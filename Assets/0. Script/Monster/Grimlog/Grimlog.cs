using System.Collections;
using UnityEngine;

public class Grimlog : MonsterBase
{
    public override void MonsterDataSetting()
    {
        monsterData.IdleTime = 3f;
        monsterData.PatrolSpeed = 3f;
        monsterData.PatrolTime = 3f;

        monsterData.MoveDirection = -1;

        monsterData.AggroRange = 10f;

        monsterData.SkillA_ActiveRange = 6f;

        monsterData.SkillA_coolTime = 15f;

        switch (selectedSkill)
        {
            case MonsterSkillType.Skill_A:
                monsterData.Skill_Damage = 10f;
                break;
        }
    }

    bool canUseSkillA = true;
    float SkillPower = 20f;

    protected override MonsterSkillType DecideSkillType()
    {
        if (!isSkillReady) return MonsterSkillType.None;

        if (DistanceToPlayer <= monsterData.SkillA_ActiveRange && canUseSkillA)
            return MonsterSkillType.Skill_A;
        else return MonsterSkillType.None;
    }

    protected override IEnumerator SkillA()
    {
        canUseSkillA = false;
        isUsingSkill = true;

        float dirX = Mathf.Sign(PlayerPosition.position.x - transform.position.x);
        monsterData.PatrolSpeed = 0f;

        yield return new WaitForSeconds(0.5f);

        rb.AddForce(new Vector2(dirX * SkillPower, 0f), ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f);

        rb.linearVelocity = new Vector2(0f, 0f);
        animator.SetTrigger("Idle");

        yield return new WaitForSeconds(3f);

        isUsingSkill = false;
        monsterData.PatrolSpeed = 3f;
        ChangeState(MonsterStateType.Aggro);

        yield return new WaitForSeconds(monsterData.SkillA_coolTime);
        canUseSkillA = true;
    }
}

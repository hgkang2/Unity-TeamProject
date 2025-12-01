using UnityEngine;

public class SkillSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.UseSkill();
    }

    public override void OnExit()
    {
        monsterBase.OnSkillExit();
    }
}

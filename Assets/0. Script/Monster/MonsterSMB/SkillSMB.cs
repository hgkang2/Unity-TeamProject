using UnityEngine;

public class SkillSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.SkillCol.SetActive(true);
        monsterBase.UseSkill();
    }

    public override void OnExit()
    {
        monsterBase.SkillCol.SetActive(false);
        monsterBase.OnSkillExit();
    }
}

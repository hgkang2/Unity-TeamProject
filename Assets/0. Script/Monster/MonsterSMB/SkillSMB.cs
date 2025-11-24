using UnityEngine;

public class SkillSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.SkillA();
    }
}

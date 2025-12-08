using UnityEngine;

public class HitSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.StopAllCoroutines();
        monsterBase.MonsterHitBox.enabled = false;
        monsterBase.isUsingSkill = false;
    }

    public override void OnExit()
    {
        monsterBase.MonsterHitBox.enabled = true;
    }
}

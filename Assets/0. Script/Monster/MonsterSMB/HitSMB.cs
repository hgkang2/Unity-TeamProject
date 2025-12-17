using UnityEngine;

public class HitSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.MonsterHitBox.enabled = false;
        monsterBase.isUsingSkill = false;
        monsterBase.spriteRenderer.color = Color.red;
    }

    public override void OnExit()
    {
        monsterBase.MonsterHitBox.enabled = true;
        monsterBase.spriteRenderer.color = Color.white;
    }
}

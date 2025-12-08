using UnityEngine;

public class HitSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.StopAllCoroutines();
        monsterBase.MonsterHitBox.enabled = false;
        monsterBase.isUsingSkill = false;
        monsterBase.spriteRenderer.color = Color.red;
        monsterBase.rb.linearVelocity = Vector2.zero;
    }

    public override void OnExit()
    {
        monsterBase.MonsterHitBox.enabled = true;
        monsterBase.spriteRenderer.color = Color.white;
    }
}

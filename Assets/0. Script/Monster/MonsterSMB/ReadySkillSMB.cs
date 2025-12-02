using UnityEngine;

public class ReadySkillSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.rb.linearVelocity = new Vector2(0f, 0f);
    }

    public override void OnUpdate()
    {
        monsterBase.spriteRenderer.color = Color.red;
    }

    public override void OnExit()
    {
        monsterBase.spriteRenderer.color = Color.white;
    }
}
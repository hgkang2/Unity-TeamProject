using UnityEngine;

public class AggroSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.rb.linearVelocity = new Vector2(0f, 0f);
        monsterBase.Alert.SetActive(true);

        if (monsterBase.direction.x > 0)
        {
            monsterBase.spriteRenderer.flipX = true;
        }
        else if (monsterBase.direction.x < 0)
        {
            monsterBase.spriteRenderer.flipX = false;
        }
    }

    public override void OnUpdate()
    {
        monsterBase.rb.linearVelocity = new Vector2(0f, 0f);
    }

    public override void OnExit()
    {
        monsterBase.Alert.SetActive(false);
    }
}
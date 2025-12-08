using UnityEngine;

public class AggroSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.rb.linearVelocity = new Vector2(0f, 0f);
        monsterBase.Alert.SetActive(true);
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
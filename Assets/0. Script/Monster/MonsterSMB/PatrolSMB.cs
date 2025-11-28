using UnityEngine;

public class PatrolSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.direction.x *= -1f;
    }
}

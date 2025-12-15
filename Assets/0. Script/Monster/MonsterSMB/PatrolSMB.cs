using UnityEngine;

public class PatrolSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.patrolDirX *= -1;
    }
}

using UnityEngine;

public class AttackSMB : MonsterStateSMB
{
    public override void OnEnter()
    {
        monsterBase.Attack();
    }

    public override void OnExit()
    {
        monsterBase.OnAttackExit();
    }
}

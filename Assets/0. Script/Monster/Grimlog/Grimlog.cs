using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Grimlog : MonsterBase
{
    public override void MonsterDataSetting()
    {
        monsterData.IdleTime = 3f;
        monsterData.PatrolSpeed = 3f;
        monsterData.PatrolTime = 3f;

        monsterData.MoveDirection = -1;

        monsterData.AggroRange = 10f;

        monsterData.SkillA_ActiveRange = 6f;

        monsterData.SkillA_coolTime = 15f;
    }

    //bool canUseSkillA = true;
    //float SkillPower = 20f;

    
}

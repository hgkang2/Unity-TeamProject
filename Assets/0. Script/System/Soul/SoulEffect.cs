using NaughtyAttributes;

[System.Serializable]
public class SoulEffect
{
    [Label("효과 타입")]
    public SoulEffectType type;

    
    [ShowIf("ShowTargetStat")]
    [AllowNesting]
    [Label("증가시킬 스탯")]
    public PlayerStatType targetStat;

    [ShowIf("ShowFlatValue")]
    [AllowNesting]
    [Label("고정 증가량")]
    public float flatValue;

    [ShowIf("ShowPercentValue")]
    [AllowNesting]
    [Label("퍼센트 증가량(%)")]
    public int percentValue;

    [ShowIf("ShowBonusValue")]
    [AllowNesting]
    [Label("보너스 증가량")]
    public float bonusValue;

    [ShowIf("ShowSkillToLearn")]
    [AllowNesting]
    [Label("습득 스킬")]
    public CharacterSkills skillToLearn;

    // 나중에 ETC가 필요해질 만큼 많이 생기면 적용
    // [ShowIf("ShowSoulEtcEffect")]
    // [AllowNesting]
    // [Label("기타 효과")]
    // public SoulETCEffect soulEtcEffect;

    [ShowIf("ShowHealAmount")]
    [AllowNesting]
    [Label("회복량")]
    public int healAmount;


    #region 영성 적용 함수
    public void ApplyOnce(Player player)
    {
        switch (type)
        {
            case SoulEffectType.StatFlat:
                player.Stats.AddStat(targetStat, flatValue, 0, 0);
                break;

            case SoulEffectType.StatPercent:
                player.Stats.AddStat(targetStat, 0, percentValue, 0);
                break;
            case SoulEffectType.StatBonus:
                player.Stats.AddStat(targetStat, 0, 0, bonusValue);
                break;

            case SoulEffectType.LearnSkill:
                //player.Skills.Learn(skillToLearn);
                break;

            case SoulEffectType.HealHP:
                player.HP.Heal(healAmount);
                break;

            case SoulEffectType.IncreaseJumpNum:
                player.playerMove.maxJumpCount++;
                break;
            case SoulEffectType.WallJump:
                player.playerMove.ActivateWallGrab();
                player.playerMove.canWallJump = true;
            break;
            // case SoulEffectType.ETC:
            //     //나중에 ETC가 필요해질 만큼 많이 생기면 적용
            //     break;
        }
    }

    #endregion


    #region 인스펙터 표시 조건
    bool ShowTargetStat()
    {
        return type == SoulEffectType.StatFlat
            || type == SoulEffectType.StatPercent
            || type == SoulEffectType.StatBonus;
    }

    bool ShowFlatValue()
    {
        return type == SoulEffectType.StatFlat;
    }

    bool ShowPercentValue()
    {
        return type == SoulEffectType.StatPercent;
    }

    bool ShowBonusValue()
    {
        return type == SoulEffectType.StatBonus;
    }

    bool ShowSkillToLearn()
    {
        return type == SoulEffectType.LearnSkill;
    }

    // 나중에 ETC가 필요해질 만큼 많이 생기면 적용
    // bool ShowSoulEtcEffect()
    // {
    //     return type == SoulEffectType.ETC;
    // }

    bool ShowHealAmount()
    {
        return type == SoulEffectType.HealHP;
    }
    #endregion
}

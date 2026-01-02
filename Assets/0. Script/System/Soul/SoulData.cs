using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "SoulData", menuName = "Scriptable Objects/SoulData")]
public class SoulData : ScriptableObject
{
    [Label("영성 ID")]
    public int index;
    [Label("영성 이미지(풀버전)")]
    public Sprite soulSprite;
    [Label("영성 이미지(아이콘)")]
    public Sprite soulIcon;
    [Label("영성 이름")]
    public string displayName;
    [Label("영성 효과")]
    public string soulEffectText = "Atk +";
    public string soulEffectText2 = "%";
    [Label("영성 설명")]
    public string soulDescript;

    [Label("영성 효과")]
    public SoulEffect effect;

    [Label("캐릭터 제한")]
    public CharacterId soulCharacterType;

    [Label("기타 조건 제한")]
    public SoulNeedType soulNeedType;

    [Label("레벨 제한")]
    public int levelConstrains = 0;

    [Label("최대 보유 개수")]
    public int maxStack = 1;
    [Label("일회용인지")]
    public bool isDisposable;





    //디폴트 설정
    void OnValidate()
    {
        displayName = name;  // ← ScriptableObject의 파일 이름
    }

    public bool CanOffer(Player player)
    {
        // 1) 캐릭터 타입 제한
        if (soulCharacterType != CharacterId.None &&
            soulCharacterType != GameManager.Instance.curcharacter)
            return false;

        // 2) 필요 조건 체크 (레벨, 특정 스킬, 특정 아이템…)
        if (!CheckNeedCondition(player))
            return false;

        // 3) 중복 Soul 제한
        //if (!isStackable && player.Stats.HasSoul(this))
        //    return false;

        return true;
    }

    private bool CheckNeedCondition(Player player)
    {
        switch (soulNeedType)
        {
            case SoulNeedType.None:
                return true;

            case SoulNeedType.LevelNeed:
                return player.Exp.CurLevel >= levelConstrains;
            case SoulNeedType.DoubleJumpGained:
                return player.playerMove.maxJumpCount == 1;
                //case SoulNeedType.MustHaveSkill:
                //    return player.HasSkill(requiredSkill);

                // 필요한 조건 계속 확장하면 됨
        }

        return true;
    }
    public int GetValue()
    {
        switch (effect.type)
        {
            case SoulEffectType.StatFlat:
                return (int)effect.flatValue;
            case SoulEffectType.StatPercent:
                return effect.percentValue;
            case SoulEffectType.IncreaseJumpNum:
                return 1;
            case SoulEffectType.LearnSkill:
                return 0;
            case SoulEffectType.HealHP:
                return -1;
        }
        return -1;
    }
}
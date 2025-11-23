using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // === Base (변하지 않는 스탯) ===
    [Header("Base Stats")]
    [SerializeField] float baseDamage = 10f;
    [SerializeField] float baseDefense = 1f;
    [SerializeField] float baseAttackSpeed = 1f;
    [SerializeField] float baseMoveSpeed = 10f;
    [SerializeField] float baseCooldown = 1f;
    [SerializeField] float baseLifeSteal = 1f;
    [SerializeField] float baseJumpForce = 5f;

    // === Current (버프/소울 적용된 현재 값) ===
    [Header("Current Stats")]
    public float curDamage;
    public float curDefense;
    public float curAttackSpeed;
    public float curMoveSpeed;
    public float curCooldown;
    public float curLifeSteal;
    public float curJumpForce;

    void Awake()
    {
        ResetToBaseStats(); // 초기값 세팅
    }

    // “현재 스탯” 계산 (획득 효과를 누적 반영)
    public void ResetToBaseStats()
    {
        curDamage = baseDamage;
        curDefense = baseDefense;
        curAttackSpeed = baseAttackSpeed;
        curMoveSpeed = baseMoveSpeed;
        curCooldown = baseCooldown;
        curLifeSteal = baseLifeSteal;
        curJumpForce = baseJumpForce;
    }

    // 외부에서 추가 보정(소울/버프)이 들어올 때
    Dictionary<PlayerStatType, float> flatBonus = new();
    Dictionary<PlayerStatType, float> percentBonus = new();
    public void AddStat(PlayerStatType type, float flat, float percent = 0f)
    {
        if (!flatBonus.ContainsKey(type))
        {
            flatBonus[type] = 0;
            percentBonus[type] = 0;
        }

        flatBonus[type] += flat;
        percentBonus[type] += percent;

        float baseVal = GetBaseValue(type);
        float final = (baseVal + flatBonus[type]) * (1 + percentBonus[type] * 0.01f);
        SetCurValue(type, final);
    }

    float GetBaseValue(PlayerStatType type)
    {
        return type switch
        {
            PlayerStatType.Damage => baseDamage,
            PlayerStatType.Defense => baseDefense,
            PlayerStatType.AttackSpeed => baseAttackSpeed,
            PlayerStatType.MoveSpeed => baseMoveSpeed,
            PlayerStatType.SkillCooldown => baseCooldown,
            PlayerStatType.LifeSteal => baseLifeSteal,
            PlayerStatType.JumpForce => baseJumpForce,
            _ => 0f
        };
    }

    void SetCurValue(PlayerStatType type, float value)
    {
        switch (type)
        {
            case PlayerStatType.Damage: curDamage = value; break;
            case PlayerStatType.Defense: curDefense = value; break;
            case PlayerStatType.AttackSpeed: curAttackSpeed = value; break;
            case PlayerStatType.MoveSpeed: curMoveSpeed = value; break;
            case PlayerStatType.SkillCooldown: curCooldown = value; break;
            case PlayerStatType.LifeSteal: curLifeSteal = value; break;
            case PlayerStatType.JumpForce: curJumpForce = value; break;
        }
    }
}

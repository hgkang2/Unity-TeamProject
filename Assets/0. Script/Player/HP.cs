using System;
using UnityEngine;

public class HP : MonoBehaviour
{
    [Header("기본 HP 설정")]
    [SerializeField] float maxHP = 100f;
    [SerializeField] float armor = 0f;            // 필요 없으면 0으로 두고 무시

    [Header("시작 시 풀피로 시작할지 여부")]
    [SerializeField] bool initializeOnAwake = true;

    [SerializeField] float curHP;
    bool isDead;

    public float CurHP { get { return curHP; } }
    public float MaxHP { get { return maxHP; } }
    public bool IsDead { get { return isDead; } }

    public event Action<float, float> OnHPChanged;
    public event Action OnDied;

    void Awake()
    {
        if (initializeOnAwake)
        {
            ResetHP();
        }
    }

    public void ResetHP()
    {
        isDead = false;
        curHP = Mathf.Max(0f, maxHP);
        RaiseHPChanged();
    }

    public void SetMaxHP(float newMax, bool fillToMax)
    {
        if (newMax < 0f)
        {
            newMax = 0f;
        }

        float differMaxHP = newMax - maxHP;
        maxHP = newMax;

        if (fillToMax)
        {
            curHP = maxHP;
        }
        else
        {
            //MaxHp가 늘어난 만큼 curHp도 증가
            //그럴 일은 없겠지만 maxHp가 줄어들면 curHp도 최대 maxHP만큼만
            curHP = Mathf.Min(curHP + Mathf.Max(differMaxHP, 0f), maxHP);
        }

        RaiseHPChanged();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        if (amount <= 0f) return;

        float finalDamage = CalculateFinalDamage(amount);
        if (finalDamage <= 0f) return;

        curHP -= finalDamage;
        if (curHP < 0f)
        {
            curHP = 0f;
        }

        

        if (curHP <= 0f)
        {
            HandleDeath();
        }
        else
        {
            RaiseHPChanged();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        if (amount <= 0f) return;

        curHP += amount;
        if (curHP > maxHP)
        {
            curHP = maxHP;
        }

        RaiseHPChanged();
    }

    public void Kill()
    {
        if (isDead) return;

        curHP = 0f;
        RaiseHPChanged();
        HandleDeath();
    }

    float CalculateFinalDamage(float rawDamage)
    {
        // 가장 단순한 방어력 처리 예시
        float reduced = rawDamage - armor;
        if (reduced < 1f)
        {
            reduced = 1f;
        }

        return reduced;
    }

    void HandleDeath()
    {
        if (isDead) return;

        isDead = true;

        if (OnDied != null)
        {
            OnDied.Invoke();
        }

        OnDiedInternal();
    }

    void RaiseHPChanged()
    {
        if (OnHPChanged != null)
        {
            OnHPChanged?.Invoke(curHP, maxHP);
        }
    }

    // 나중에 플레이어용/보스용에서 상속으로 커스텀하고 싶으면
    // HPPlayer : HP 해서 이 메서드 오버라이드
    protected virtual void OnDiedInternal()
    {
        // 기본 구현: 아무것도 안 함
        // 몬스터는 여기서 애니/파티클/풀 반환 같은 것 할 수 있음
    }
}

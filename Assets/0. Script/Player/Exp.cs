using System;
using UnityEngine;

public class Exp : MonoBehaviour
{
    [Header("레벨업에 필요한 경험치 테이블")]
    [SerializeField] int[] levelMaxExpTable = new int[10];

    [Header("현재 상태")]
    [SerializeField] int curlevel = 1;
    [SerializeField] int curExp = 0;

    public int CurLevel => curlevel;
    public int CurExp => curExp;

    public int MaxExp
    {
        get
        {
            int index = Mathf.Clamp(curlevel - 1, 0, levelMaxExpTable.Length - 1);
            return levelMaxExpTable[index];
        }
    }

    // UI 등에서 사용
    public event Action<int> LevelChanged;
    public event Action<int, int> ExpChanged; // (curExp, maxExp)

    
    [SerializeField] AudioClip levelUpSound;


    void Awake()
    {
        //널가드
        if (levelMaxExpTable.Length == 0)
        {
            Debug.Log("레벨테이블 값 입력 하기"); return;
        }
        foreach (int i in levelMaxExpTable)
        {
            if (i == 0) { Debug.Log("레벨테이블 값 입력 하기"); return; }
        }
    }

    // 경험치 추가
    public void AddExp(int amount)
    {
        if (amount <= 0)
            return;

        curExp += amount;

        // 연속 레벨업 처리
        while (curExp >= MaxExp)
        {
            curExp -= MaxExp;
            LevelUp();
        }

        // UI 반영
        ExpChanged?.Invoke(curExp, MaxExp);
    }

    public void LevelUp(int value = 1){
        curlevel += value;
        //최대 레벨보다 더 높아지면 최대레벨로
        if(curlevel > levelMaxExpTable.Length){
            curlevel = levelMaxExpTable.Length;
        }
        LevelChanged?.Invoke(curlevel);
        SoundManager.Instance.PlaySFX(levelUpSound);
    }


    #region Save&Load
    public ExpData SaveExp()
    {
        return new ExpData
        {
            level = this.curlevel,
            curExp = this.curExp
        };
    }

    public void LoadExp(ExpData data)
    {
        this.curlevel = data.level;
        this.curExp = data.curExp;
    }
    #endregion
}

[Serializable]
public class ExpData
{
    public int level;
    public int curExp;
}

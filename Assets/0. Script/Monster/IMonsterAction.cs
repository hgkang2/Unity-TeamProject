using UnityEngine;

public enum MonsterActionType { Attack, Skill }

public interface IMonsterAction 
{
    MonsterActionType Type { get; }
    int Priority { get; } // 큰 값우선 실행
    bool IsRunning { get; } // 실행 여부
    bool IsReady { get; } // 쿨타임

    bool Canstart(MonsterBase monsterBase);
    void Trigger(MonsterBase monsterBase);

    void OnEnter(MonsterBase monsterBase);
    void OnExit(MonsterBase monsterBase);
}

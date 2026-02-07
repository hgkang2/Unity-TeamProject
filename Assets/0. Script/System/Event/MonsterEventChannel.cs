using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventChannel/MonsterEventChannel")]

public class MonsterEventChannel : ScriptableObject
{
    public event Action<MonsterType> monsterDead;
    public void RaiseMonsterDead(MonsterType type) => monsterDead?.Invoke(type);
}

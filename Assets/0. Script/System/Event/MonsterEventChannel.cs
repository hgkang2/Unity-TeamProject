using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventChannel/MonsterEventChannel")]

public class MonsterEventChannel : ScriptableObject
{
    public event Action monsterDead;
    public void RaiseMonsterDead() => monsterDead?.Invoke();
}

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventChannel/PlayerEventChannel")]
public class PlayerEventChannel : ScriptableObject
{
    public event Action<AttackType> playerAttacked;
    public event Action playerJumped;

    public void RaisePayerAttacked(AttackType type) => playerAttacked?.Invoke(type);
    public void RaiseItemUsed() => playerJumped?.Invoke();
}

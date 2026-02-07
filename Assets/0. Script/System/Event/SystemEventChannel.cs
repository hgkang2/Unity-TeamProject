using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventChannel/SystemEventChannel")]

public class SystemEventChannel : ScriptableObject
{
    public event Action AltarActivate;
    public void RaiseAltarActivate() => AltarActivate?.Invoke();
}
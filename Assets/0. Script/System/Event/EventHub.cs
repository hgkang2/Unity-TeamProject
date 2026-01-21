using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventHub")]
public class EventHub : ScriptableObject
{
    public PlayerEventChannel player;
    public MonsterEventChannel monster;
}

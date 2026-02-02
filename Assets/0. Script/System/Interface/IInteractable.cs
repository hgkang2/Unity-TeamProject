using UnityEngine;

public interface IInteractable
{
    Transform InteractionUIPosition { get; }
    bool CanInteract();     // 스캔 대상인지 (상황/상태)
    bool IsAvailable();     // 실제 상호작용 가능한지 (조건/잠금 등)

    void OnFocus();
    void OnUnfocus();

    void Interact(Player user, Interactor interactor);
    void Exit();
}

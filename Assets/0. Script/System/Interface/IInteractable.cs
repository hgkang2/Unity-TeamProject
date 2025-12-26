using UnityEngine;

public interface IInteractable
{
    bool CanInteract();     // 스캔 대상인지 (상황/상태)
    bool IsAvailable();     // 실제 상호작용 가능한지 (조건/잠금 등)

    Vector2 GetInteractPoint(); // 거리 판정 기준점

    void OnFocus();
    void OnUnfocus();

    void Interact(Player user);
    void Exit();
}

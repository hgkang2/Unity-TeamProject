using UnityEngine;

public abstract class UIKeyboardHandler : MonoBehaviour
{
    [Header("UI 키보드 반복 설정")]
    [SerializeField] float initialDelay = 0.4f;    // 첫 입력 후 반복까지
    [SerializeField] float repeatInterval = 0.08f; // 그 이후 반복 간격

    Vector2 heldDir;
    bool isHeld;
    float nextRepeatTime;

    void OnEnable()
    {
        SubscribeInputEvent();
        //자식에서 OnEnable 구현하고 싶으면 이걸로
        OnUIEnabled();
    }

    void OnDisable()
    {
        UnSubscribeInputEvent();
        //자식에서 Ondisable 구현하고 싶으면 이걸로
        OnUIDisabled();
    }

    protected void SubscribeInputEvent()
    {
        UnSubscribeInputEvent();
        InputManager.Instance.UINavigateStarted += HandleNavigateStarted;
        InputManager.Instance.UINavigateCanceled += HandleNavigateCanceled;

        InputManager.Instance.UICanceled += HandleUICancel;
        InputManager.Instance.UIConfirmed += HandleUIConfirm;
    }

    protected void UnSubscribeInputEvent()
    {
        InputManager.Instance.UINavigateStarted -= HandleNavigateStarted;
        InputManager.Instance.UINavigateCanceled -= HandleNavigateCanceled;

        InputManager.Instance.UICanceled -= HandleUICancel;
        InputManager.Instance.UIConfirmed -= HandleUIConfirm;
    }

    void Update()
    {
        if (!isHeld || heldDir == Vector2.zero)
            return;

        if (Time.unscaledTime >= nextRepeatTime)
        {
            OnUIMove(heldDir);
            nextRepeatTime = Time.unscaledTime + repeatInterval;
        }
    }

    void HandleNavigateStarted(Vector2 dir)
    {
        heldDir = dir;
        isHeld = true;

        // 첫 입력: 즉시 한 칸
        OnUIMove(dir);

        // 이후: 반복 시점 예약
        nextRepeatTime = Time.unscaledTime + initialDelay;
    }

    void HandleNavigateCanceled()
    {
        isHeld = false;
        heldDir = Vector2.zero;
    }

    void HandleUIConfirm()
    {
        OnUIConfirm();
    }

    void HandleUICancel()
    {
        OnUICancel();
    }

    // --- 자식에서 구현할 것들 ---

    // 방향 이동 (한 칸)
    protected abstract void OnUIMove(Vector2 dir);

    // 확인 / 취소는 필요할 때만 override
    protected virtual void OnUIConfirm() { }
    protected virtual void OnUICancel() { }

    //자식에서 OnEnable, OnDisable 구현하고 싶을 때
    protected virtual void OnUIEnabled() { }
    protected virtual void OnUIDisabled() { }
}

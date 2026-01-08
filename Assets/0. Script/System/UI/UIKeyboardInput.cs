using UnityEngine;

public class UIKeyboardInput : MonoBehaviour
{

    [SerializeField] float initialDelay = 0.4f;
    [SerializeField] float repeatInterval = 0.08f;

    Vector2 heldDir;
    bool isHeld;
    float nextRepeatTime;

    IUIKeyboardTarget target;

    void Awake()
    {
        // 같은 GameObject에 붙어있는 IUIKeyboardTarget 함수를 실행
        target = GetComponent<IUIKeyboardTarget>();
    }

    void OnEnable()
    {
        SubscribeInputEvent();
    }

    void OnDisable()
    {
        UnSubscribeInputEvent();
    }

    void Update()
    {
        if (!isHeld || heldDir == Vector2.zero)
            return;

        if (Time.unscaledTime >= nextRepeatTime)
        {
            target?.OnUIMove(heldDir);
            nextRepeatTime = Time.unscaledTime + repeatInterval;
        }
    }
    public void SetInputTarget(IUIKeyboardTarget newTarget)
    {
        target = newTarget;
    }

    void HandleNavigateStarted(Vector2 dir)
    {
        heldDir = dir;
        isHeld = true;

        target?.OnUIMove(dir);
        nextRepeatTime = Time.unscaledTime + initialDelay;
    }

    void HandleNavigateCanceled()
    {
        isHeld = false;
        heldDir = Vector2.zero;
    }

    void HandleUIConfirm()
    {
        target?.OnUIConfirm();
    }

    void HandleUICancel()
    {
        target?.OnUICancel();
    }

    void SubscribeInputEvent()
    {
        InputManager.Instance.UINavigateStarted += HandleNavigateStarted;
        InputManager.Instance.UINavigateCanceled += HandleNavigateCanceled;
        InputManager.Instance.UIConfirmed += HandleUIConfirm;
        InputManager.Instance.UICanceled += HandleUICancel;
    }

    void UnSubscribeInputEvent()
    {
        InputManager.Instance.UINavigateStarted -= HandleNavigateStarted;
        InputManager.Instance.UINavigateCanceled -= HandleNavigateCanceled;
        InputManager.Instance.UIConfirmed -= HandleUIConfirm;
        InputManager.Instance.UICanceled -= HandleUICancel;
    }
}

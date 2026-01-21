using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIPanelBase : MonoBehaviour, IUIKeyboardTarget
{
    [Header("Input")]
    [SerializeField] bool blocksGameplay = true;

    public bool IsOpen { get; private set; }
    protected virtual bool CanClose => true;
    protected bool ignoreInput;
    protected CanvasGroup cg;

    protected void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        Init();
    }

    protected virtual void Init() { }

#if UNITY_EDITOR
    void OnDisable()
    {
        if (IsOpen) InputManager.Instance.RemoveUI(this, blocksGameplay);
    }
#endif

    public void Open()
    {
        if (IsOpen)
        {
            ShowCG(true);
            return;
        }

        IsOpen = true;
        ShowCG(true);
        ResetFocus();
        InputManager.Instance.PushUI(this, blocksGameplay);
        OnOpened();
    }

    public void Close()
    {
        if (!IsOpen)
        {
            ShowCG(false);
            return;
        }
        IsOpen = false;
        OnClosing();

        if (blocksGameplay)
        {
            // 모달 UI : LIFO 강제
            InputManager.Instance.PopUI(this, true);
        }
        else
        {
            // 비모달 UI : Top이면 Pop, 아니면 Remove
            if (ReferenceEquals(InputManager.Instance.TopUI, this))
                InputManager.Instance.PopUI(this, false);
            else
                InputManager.Instance.RemoveUI(this, false);
        }

        ShowCG(false);
    }

    void ShowCG(bool visible)
    {
        cg.alpha = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
        cg.interactable = visible;
        enabled = visible;
    }
    

    // --- IUIKeyboardTarget 기본 동작 ---
    public virtual void OnUIInputMove(Vector2 dir)
    {
        if (ignoreInput) return;
    }

    public virtual void OnUIInputConfirm() { }

    //OnUICancel = 취소 입력이 들어왔을 때(닫을지/다른 행동할지 결정 단계)

    
    public virtual void OnUIInputCancel()
    {
        if (!CanClose) return;
        Close();
    }

    // --- 자식에서 추가 기능 필요시 ---
    protected virtual void ResetFocus() { }
    protected virtual void OnOpened() { }
    //OnClosing = 이미 닫기로 결정된 뒤(정리/연출 단계)
    protected virtual void OnClosing() { }

}

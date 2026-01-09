using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIPanelBase : MonoBehaviour, IUIKeyboardTarget
{
    [Header("Input")]
    [SerializeField] bool blocksGameplay = true;

    protected CanvasGroup cg;

    protected virtual void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public virtual void Open()
    {
        ShowCG(true);

        InputManager.Instance.PushUI(this, blocksGameplay);

        OnOpened();
    }

    public virtual void Close()
    {
        OnClosing();

        InputManager.Instance.PopUI(this, blocksGameplay);

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
    public abstract void OnUIMove(Vector2 dir);

    public virtual void OnUIConfirm() { }

    public virtual void OnUICancel()
    {
        // 기본 Cancel = 닫기
        Close();
    }

    // --- Hook ---
    protected virtual void OnOpened() { }
    protected virtual void OnClosing() { }

}

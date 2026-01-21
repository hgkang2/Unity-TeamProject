using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfirmPanel : UIPanelBase
{
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    IFocusableView confirmView;
    IFocusableView cancelView;

    Button curButton;

    protected override void Init()
    {
        // Button에 IFocusableView가 붙어 있으면 포커스 비주얼을 사용
        confirmView = confirmButton.GetComponent<IFocusableView>();
        cancelView = cancelButton.GetComponent<IFocusableView>();
    }

    public override void OnUIInputMove(Vector2 dir)
    {
        base.OnUIInputMove(dir);

        if (dir.x < -0.1f)
            SetCurrent(confirmButton, confirmView);
        else if (dir.x > 0.1f)
            SetCurrent(cancelButton, cancelView);
    }

    void SetCurrent(Button btn, IFocusableView view)
    {
        // 0) 선택 해제
        if (btn == null)
        {
            ClearCurrent();
            return;
        }

        // 1) 동일 버튼이면 무시
        if (curButton == btn) return;
        curButton = btn;

        // 2) 커스텀 뷰가 하나라도 있으면 그걸로 표시
        if (confirmView != null || cancelView != null)
        {
            confirmView?.SetFocused(false);
            cancelView?.SetFocused(false);
            view?.SetFocused(true);
            return;
        }

        // 3) 커스텀 뷰가 전혀 없으면 기본 선택 표시
        curButton.Select();
    }
    public void SetCurrent(Button btn)
    {
        if (btn == null)
        {
            SetCurrent(null, null);
            return;
        }

        if (btn == confirmButton) SetCurrent(confirmButton, confirmView);
        else if (btn == cancelButton) SetCurrent(cancelButton, cancelView);
    }

    void ClearCurrent()
    {
        curButton = null;

        // 커스텀 포커스 해제
        confirmView?.SetFocused(false);
        cancelView?.SetFocused(false);

        // 기본 Select fallback 정리
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public override void OnUIInputConfirm()
    {
        if (curButton != null) curButton.onClick.Invoke();
    }

    public override void OnUIInputCancel()
    {
        ClearCurrent();
        base.OnUIInputCancel();
    }
}

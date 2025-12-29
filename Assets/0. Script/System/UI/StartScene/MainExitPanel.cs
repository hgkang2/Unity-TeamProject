using UnityEngine;
using UnityEngine.UI;

public class MainExitPanel : UIKeyboardHandler
{
    public CanvasGroup cg;
    [SerializeField] Button ConfirmButton;
    [SerializeField] Button CancelButton;
    protected override void OnUIMove(Vector2 dir)
    {

    }

    protected override void OnUICancel()
    {
    }

    protected override void OnUIConfirm()
    {
    }
    public void Open()
    {
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        enabled = true;
    }

    public void Close()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        enabled = false;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class StartExitPanel : UIKeyboardHandler
{
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
}

using UnityEngine;

public interface IUIKeyboardTarget
{
    void OnUIMove(Vector2 dir);
    void OnUIConfirm();
    void OnUICancel();
}

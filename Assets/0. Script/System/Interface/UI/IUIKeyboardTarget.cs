using UnityEngine;

public interface IUIKeyboardTarget
{
    void OnUIInputMove(Vector2 dir);
    void OnUIInputConfirm();
    void OnUIInputCancel();
}

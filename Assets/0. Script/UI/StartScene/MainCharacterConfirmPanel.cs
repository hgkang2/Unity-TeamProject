using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainCharacterConfirmPanel : UIPanelBase
{
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    Button curButton;

    public override void OnUIInputMove(Vector2 dir)
    {
        base.OnUIInputMove(dir);

        //왼쪽 방향키시 예 버튼
        if (dir.x < -0.1f) curButton = confirmButton;
        //오른쪽 방향키시 아니오 버튼
        else if (dir.x > 0.1f) curButton = cancelButton;

        curButton.Select();
    }


    public override void OnUIInputConfirm()
    {
        if (curButton != null) curButton.onClick.Invoke();
    }

    public override void OnUIInputCancel()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        curButton = null;
        base.OnUIInputCancel();
    }
}

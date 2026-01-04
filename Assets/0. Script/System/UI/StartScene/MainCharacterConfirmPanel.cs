using TMPro;
using UnityEditor.Rendering.CustomRenderTexture.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;

public class MainCharacterConfirmPanel : UIKeyboardHandler
{
    [HideInInspector] public CanvasGroup cg;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    Button curButton;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    protected override void OnUIMove(Vector2 dir)
    {
        //왼쪽 방향키시 예 버튼
        if (dir.x < -0.1f)
        {
            curButton = confirmButton;
        }
        //오른쪽 방향키시 아니오 버튼
        else if(dir.x > 0.1f)
        {
            curButton = cancelButton;
        }
        curButton.Select();
    }

    protected override void OnUIConfirm()
    {
        if (curButton != null)
        {
            curButton.onClick.Invoke();
        }
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
        curButton = null;

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        enabled = false;
    }
}

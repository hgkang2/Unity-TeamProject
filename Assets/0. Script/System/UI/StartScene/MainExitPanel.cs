using System;
using UnityEngine;
using UnityEngine.UI;

public class MainExitPanel : MonoBehaviour, IUIKeyboardTarget
{
    public CanvasGroup cg;
    [SerializeField] Button ConfirmButton;
    [SerializeField] Button CancelButton;
    
    

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
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

    void IUIKeyboardTarget.OnUIMove(Vector2 dir)
    {
        
    }

    void IUIKeyboardTarget.OnUIConfirm(){}
    void IUIKeyboardTarget.OnUICancel(){}
}

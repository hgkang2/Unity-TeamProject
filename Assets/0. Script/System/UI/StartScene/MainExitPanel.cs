using UnityEngine;
using UnityEngine.UI;

public class MainExitPanel : UIKeyboardHandler
{
    public CanvasGroup cg;
    [SerializeField] Button ConfirmButton;
    [SerializeField] Button CancelButton;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }
    protected override void OnUIMove(Vector2 dir)
    {
        //좌우 방향키로 예/아니오 선택
        //if (dir.x < -0.1f) ;
        //else if (dir.x > 0.1f) ;
    }

    protected override void OnUIConfirm()
    {
        // 상호작용 키 눌러서 선택된 버튼 확인
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

using UnityEngine;
using UnityEngine.UI;

public class StartPanel : UIKeyboardHandler
{
    [SerializeField] Button[] menuButtons;
    [SerializeField] GameObject exitPanel;
    int? curIndex = null;

    void Start()
    {
        exitPanel.SetActive(false);
        UpdateHighlight();
    }


    protected override void OnUIMove(Vector2 dir)
    {
        // 현재 아무것도 선택되지 않은 상태라면
        if (curIndex == null)
        {
            // 왼쪽/위쪽 → 0번 선택
            if (dir.x < -0.1f || dir.y > 0.1f) curIndex = 0;
            // 오른쪽/아래쪽 → 마지막 선택
            else if (dir.x > 0.1f || dir.y < -0.1f) curIndex = menuButtons.Length - 1;

            UpdateHighlight();
            return;
        }

        //왼쪽 or 위쪽 방향키시 위쪽 방향으로
        if (dir.x < -0.1f || dir.y > 0.1) curIndex--;
        //오른쪽 or 아래쪽 방향키시 아래쪽 방향으로
        else if (dir.x > 0.1f || dir.y < -0.1) curIndex++;

        //min, max 처리
        if (curIndex < 0) curIndex = menuButtons.Length - 1;
        else if (curIndex >= menuButtons.Length) curIndex = 0;

        //강조된 버튼 변경
        UpdateHighlight();
    }

    void UpdateHighlight()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            Button btn = menuButtons[i];

            // 강조 처리 or default 상태로 되돌리기
            bool selected = (i == curIndex);
            ColorBlock colors = btn.colors;
            colors.normalColor = selected ? Color.yellow : Color.white;
            btn.colors = colors;
        }
    }

    protected override void OnUICancel()
    {
        //종료확인창 이라면 우선 끄기
        if (exitPanel.activeSelf == true)
        {
            exitPanel.SetActive(false);
        }
        // 버튼이 선택된 상태면 해제
        else if (curIndex != null)
        {
            curIndex = null;
            UpdateHighlight();
        }
        // 종료 하시겠습니까? UI 띄우기
        else
        {
            exitPanel.SetActive(true);
        }
    }

    protected override void OnUIConfirm()
    {
        // 선택이 없으면 무시
        if (curIndex == null) return; 
        // 마우스로 클릭한 것과 동일하게 실행
        menuButtons[curIndex.Value].onClick.Invoke();
    }

    public void Quit()
    {
        GameManager.Instance.QuitGame();
    }
}

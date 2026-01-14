using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : UIPanelBase
{
    Button[] menuButtons;
    [SerializeField] Transform ButtonSelectImage;
    [SerializeField] MainSaveDataPanel mainSaveDataPanel;
    [SerializeField] MainExitPanel mainExitPanel;


    protected override void Init()
    {
        // 활성화 된 메뉴 버튼만 캐싱 하기(인스펙터에서 설정)
        int i = 0;
        menuButtons = transform.Cast<Transform>()
            .Select(t =>
            {
                Button button = t.GetComponent<Button>();
                StartButtonEvent evt = t.GetComponent<StartButtonEvent>();

                if (evt == null) return null;
                if (!evt.activate) return null;

                evt.SetIndex(i++);
                evt.onEnter += ButtonMouseEnter;
                evt.onExit += ButtonMouseExit;
                return button;
            })
            .Where(b => b != null)
            .ToArray();

        mainExitPanel.gameObject.SetActive(true);
        mainSaveDataPanel.gameObject.SetActive(true);
        ButtonSelectImage.gameObject.SetActive(false);

        mainExitPanel.Close();
        mainSaveDataPanel.Close();
    }

    void OnDestroy()
    {

        for (int i = 0; i < menuButtons.Length; i++)
        {
            StartButtonEvent evt = menuButtons[i].GetComponent<StartButtonEvent>();
            if (evt == null) continue;

            evt.onEnter -= ButtonMouseEnter;
            evt.onExit -= ButtonMouseExit;
        }
    }

    public void OpenMainSaveDataPanel()
    {
        mainSaveDataPanel.Open();
    }

    int? curIndex = null;
    void UpdateButtonHighlight()
    {
        if (curIndex == null)
        {
            ButtonSelectImage.gameObject.SetActive(false);
        }
        else
        {
            ButtonSelectImage.gameObject.SetActive(true);
            ButtonSelectImage.transform.position = menuButtons[(int)curIndex].transform.position;
            //menuButtons[(int)curIndex].GetComponentsInChildren<>;
        }
    }

    public void ButtonMouseEnter(int index)
    {
        curIndex = index;
        UpdateButtonHighlight();
    }

    public void ButtonMouseExit()
    {
        ButtonSelectImage.gameObject.SetActive(false);
    }


    public override void OnUIInputMove(Vector2 dir)
    {
        base.OnUIInputMove(dir);
        // 현재 아무것도 선택되지 않은 상태라면
        if (curIndex == null)
        {
            // 위쪽 → 0번 선택
            if (dir.y > 0.1f) curIndex = 0;
            // 아래쪽 → 마지막 선택
            else if (dir.y < -0.1f) curIndex = menuButtons.Length - 1;

            UpdateButtonHighlight();
            return;
        }

        //위쪽 방향키시 위쪽 방향으로
        if (dir.y > 0.1) curIndex--;
        //아래쪽 방향키시 아래쪽 방향으로
        else if (dir.y < -0.1) curIndex++;

        //min, max 처리
        if (curIndex < 0) curIndex = 0;
        else if (curIndex >= menuButtons.Length) curIndex = menuButtons.Length - 1;

        //강조된 버튼 변경
        UpdateButtonHighlight();
    }

    public override void OnUIInputConfirm()
    {
        //선택 버튼이 없으면 무시
        if (curIndex == null) return;
        // 마우스로 클릭한 것과 동일하게 실행
        menuButtons[curIndex.Value].onClick.Invoke();
    }

    // 닫혀버리면 안 돼서 OnClosing에 추가구현 대신 OnUIInputCancel 덮어쓰기
    public override void OnUIInputCancel()
    {
        // 버튼이 선택된 상태면 해제
        if (curIndex != null)
        {
            curIndex = null;
            UpdateButtonHighlight();
        }
        mainExitPanel.Open();
    }
}

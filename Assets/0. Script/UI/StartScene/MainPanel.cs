using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : UIPanelBase
{
    [SerializeField] MainPanelButton[] buttons;
    [SerializeField] Transform buttonSelectImage;
    [SerializeField] MainSaveDataPanel mainSaveDataPanel;
    [SerializeField] MainSettingPanel mainSettingPanel;
    [SerializeField] ConfirmPanel mainExitPanel;

    public int? curIndex = null;
protected override void Init()
{
    // 비활성 자식까지 포함 (빌드/초기상태에서 buttons가 0개 되는 케이스 방지)
    buttons = GetComponentsInChildren<MainPanelButton>(true);

    // 필수 참조 누락이면 입력 콜백에서 NRE 연쇄 발생 → 게임 전체 입력이 망가질 수 있음
    if (buttonSelectImage == null || mainExitPanel == null || mainSaveDataPanel == null || mainSettingPanel == null)
    {
        Debug.LogError(
            $"[MainPanel] Missing refs.\n" +
            $"- buttonSelectImage: {(buttonSelectImage ? "OK" : "NULL")}\n" +
            $"- mainExitPanel: {(mainExitPanel ? "OK" : "NULL")}\n" +
            $"- mainSaveDataPanel: {(mainSaveDataPanel ? "OK" : "NULL")}\n" +
            $"- mainSettingPanel: {(mainSettingPanel ? "OK" : "NULL")}"
        );
        gameObject.SetActive(false);
        return;
    }

    mainExitPanel.gameObject.SetActive(true);
    mainSaveDataPanel.gameObject.SetActive(true);
    mainSettingPanel.gameObject.SetActive(true);
    buttonSelectImage.gameObject.SetActive(false);
}

    protected override void OnOpened()
    {
        mainExitPanel.Close();
        mainSaveDataPanel.Close();
        mainSettingPanel.Close();
    }

    public void OpenMainSaveDataPanel()
    {
        mainSaveDataPanel.Open();
    }
    public void OpenMainSettingPanel()
    {
        mainSettingPanel.Open();
    }


    public void ButtonMouseEnter(MainPanelButton hoverButton)
    {
        if (buttons == null || buttons.Length == 0) return;
        if (hoverButton == null) return;

        int i = 0;
        for (; i < buttons.Length; i++)
        {
            if (buttons[i] == hoverButton) break;
        }

        // 못 찾았으면(curIndex == buttons.Length) 인덱스 범위 밖 접근 방지
        if (i >= buttons.Length) return;

        curIndex = i;
        UpdateButtonHighlight();
    }

    public void ButtonMouseExit()
    {
        curIndex = null;
        ClearFocus();
    }


    void UpdateButtonHighlight()
    {
        ClearFocus();

        if (curIndex != null)
        {
            buttons[curIndex.Value].Focused();
            buttonSelectImage.gameObject.SetActive(true);
            buttonSelectImage.transform.position = buttons[curIndex.Value].transform.position;
        }
    }
    void ClearFocus()
    {
        // 빌드에서 참조 누락/초기화 타이밍 이슈가 있어도 입력 콜백을 깨지 않도록 방어
        if (buttonSelectImage != null)
            buttonSelectImage.gameObject.SetActive(false);

        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            if (b != null) b.UnFocused();
        }
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
            else if (dir.y < -0.1f) curIndex = buttons.Length - 1;

            UpdateButtonHighlight();
            return;
        }

        //위쪽 방향키시 위쪽 방향으로
        if (dir.y > 0.1) curIndex--;
        //아래쪽 방향키시 아래쪽 방향으로
        else if (dir.y < -0.1) curIndex++;

        //min, max 처리
        if (curIndex < 0) curIndex = 0;
        else if (curIndex >= buttons.Length) curIndex = buttons.Length - 1;

        //강조된 버튼 변경
        UpdateButtonHighlight();
    }

    public override void OnUIInputConfirm()
    {
        //선택 버튼이 없으면 무시
        if (curIndex == null) return;
        // 마우스로 클릭한 것과 동일하게 실행
        buttons[curIndex.Value].Confirm();
    }

    // 닫혀버리면 안 돼서 OnClosing에 추가구현 대신 OnUIInputCancel 덮어쓰기
    public override void OnUIInputCancel()
    {
        // 버튼이 선택된 상태면 해제
        if (curIndex != null)
        {
            curIndex = null;
            ClearFocus();
        }
        mainExitPanel.Open();
    }
}

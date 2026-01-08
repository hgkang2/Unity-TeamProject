using System.Data.Common;
using System.Linq;
using DG.Tweening;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour, IUIKeyboardTarget
{
    [SerializeField] Image openingImage;

    [SerializeField] MainPanel mainPanel;
    [SerializeField] MainSaveDataPanel mainSaveDataPanel;
    [SerializeField] MainCharacterChoicePanel mainCharacterChoicePanel;
    [SerializeField] MainCharacterConfirmPanel mainCharacterConfirmPanel;
    [SerializeField] MainExitPanel mainExitPanel;

    UIKeyboardInput keyboardInput;
    readonly System.Collections.Generic.Stack<MonoBehaviour> uiStack = new();

    Sequence openingSequence;

    bool isOpeningPhase = true;

    void Awake()
    {
        keyboardInput = GetComponent<UIKeyboardInput>();
        mainPanel.gameObject.SetActive(true);
        mainSaveDataPanel.gameObject.SetActive(true);
        mainCharacterChoicePanel.gameObject.SetActive(true);
        mainCharacterConfirmPanel.gameObject.SetActive(true);
        mainExitPanel.gameObject.SetActive(true);
    }

    void Start()
    {
        // 다른 UI들 숨긴 상태로 시작
        mainPanel.Close();
        mainSaveDataPanel.Close();
        mainCharacterChoicePanel.Close();
        mainCharacterConfirmPanel.Close();
        mainExitPanel.Close();

        // 오프닝은 보이게
        openingImage.gameObject.SetActive(true);
        keyboardInput.SetInputTarget(this);

        OpeningStart();
    }


    #region 오프닝 재생
    public void OpeningStart()
    {
        openingSequence = DOTween.Sequence();
        openingSequence.AppendInterval(2f);

        // 1) Opening fade-out
        openingSequence.Append(
            openingImage.DOFade(0, 1f)
        );

        // 2) 잠깐 대기
        openingSequence.AppendInterval(0.5f);

        // 3) 메뉴 fade-in
        openingSequence.Append(
            mainPanel.cg.DOFade(1f, 1f)
                .OnComplete(() => { ShowMenuImmediate(); })
        );
    }
    #endregion

    #region 오프닝 종료, 메인화면 시작
    // 메뉴를 최종 상태로 만드는 공용 함수 (페이드 완료 or 스킵)
    void ShowMenuImmediate()
    {
        if (openingSequence != null && openingSequence.IsActive())
            openingSequence.Kill();

        isOpeningPhase = false;
        openingImage.gameObject.SetActive(false);

        Push(mainPanel);

        SoundManager.Instance.PlayBGM("MainTheme");
    }
    #endregion


    #region UI 입력 이벤트들

    public void UICancel()
    {
        // 오프닝이면 스킵
        if (isOpeningPhase)
        {
            ShowMenuImmediate();
            return;
        }

        // 메인(top)이면: 닫는 대신 Exit 열기
        if (uiStack.Count > 0 && ReferenceEquals(uiStack.Peek(), mainPanel))
        {
            Push(mainExitPanel);
            return;
        }

        // 그 외에는 그냥 최상단 닫기
        Pop();
    }
    #endregion

    public void OpenMainSaveDataPanel()
    {
        Push(mainSaveDataPanel);
    }
    public void Quit()
    {
        GameManager.Instance.QuitGame();
    }

    void IUIKeyboardTarget.OnUIMove(Vector2 dir)
    {

    }

    public void OnUIConfirm()
    {

    }

    void IUIKeyboardTarget.OnUICancel()
    {
        UICancel();
    }

    #region UI Stack

    void Push(MonoBehaviour panel)
    {
        // 중복 push 방지: 이미 top이면 앞으로만
        if (uiStack.Count > 0 && ReferenceEquals(uiStack.Peek(), panel))
        {
            BringToFront(panel);
            RefreshTopTarget();
            return;
        }

        // 패널 열기 (기존 Open() 사용)
        OpenPanel(panel);

        uiStack.Push(panel);

        BringToFront(panel);
        RefreshTopTarget();
    }

    bool Pop()
    {
        if (uiStack.Count == 0)
            return false;

        var top = uiStack.Pop();
        ClosePanel(top);

        RefreshTopTarget();
        return true;
    }

    void ReplaceTop(MonoBehaviour panel)
    {
        // “현재 top 닫고 새거 열기” 같은 경우에 사용
        if (uiStack.Count > 0)
            ClosePanel(uiStack.Pop());

        OpenPanel(panel);
        uiStack.Push(panel);

        BringToFront(panel);
        RefreshTopTarget();
    }

    void RefreshTopTarget()
    {
        // 오프닝 중이면 StartUI가 cancel만 처리하도록 StartUI를 타겟으로 둬도 되고,
        // 아니면 top panel이 IUIKeyboardTarget이면 그걸 타겟으로 둔다.
        IUIKeyboardTarget target = this;

        if (!isOpeningPhase && uiStack.Count > 0)
        {
            var top = uiStack.Peek();
            var topTarget = top as IUIKeyboardTarget;
            if (topTarget != null)
                target = topTarget;
        }

        keyboardInput.SetInputTarget(target);
    }

    void BringToFront(MonoBehaviour panel)
    {
        panel.transform.SetAsLastSibling();
    }

    // ---- 패널별 Open/Close 호출 매핑 ----
    // (각 패널에 Open/Close가 이미 있으니 여기서만 호출한다)

    void OpenPanel(MonoBehaviour panel)
    {
        switch (panel)
        {
            case MainPanel p:
                p.Open();
                break;
            case MainSaveDataPanel p:
                p.Open();
                break;
            case MainCharacterChoicePanel p:
                p.Open();
                break;
            case MainCharacterConfirmPanel p:
                p.Open();
                break;
            case MainExitPanel p:
                p.Open();
                break;
            default:
                // 패널 추가되면 여기에 case 추가
                panel.gameObject.SetActive(true);
                panel.enabled = true;
                break;
        }
    }

    void ClosePanel(MonoBehaviour panel)
    {
        switch (panel)
        {
            case MainPanel p:
                p.Close();
                break;
            case MainSaveDataPanel p:
                p.Close();
                break;
            case MainCharacterChoicePanel p:
                p.Close();
                break;
            case MainCharacterConfirmPanel p:
                p.Close();
                break;
            case MainExitPanel p:
                p.Close();
                break;
            default:
                panel.enabled = false;
                panel.gameObject.SetActive(false);
                break;
        }
    }

    #endregion
}
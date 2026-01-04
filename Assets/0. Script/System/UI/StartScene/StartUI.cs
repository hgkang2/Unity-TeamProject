using System.Data.Common;
using System.Linq;
using DG.Tweening;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : UIKeyboardHandler
{
    [SerializeField] Image openingImage;

    [SerializeField] MainPanel mainPanel;
    [SerializeField] MainSaveDataPanel mainSaveDataPanel;
    [SerializeField] MainCharacterChoicePanel mainCharacterChoicePanel;
    [SerializeField] MainCharacterConfirmPanel mainCharacterConfirmPanel;
    [SerializeField] MainExitPanel mainExitPanel;

    Sequence openingSequence;

    bool isOpeningPhase = true;

    void Awake()
    {
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
        OpeningStart();
    }

    private void OnDisable() {
        SoundManager.Instance.StopBGM();
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

        mainPanel.Open();

        SoundManager.Instance.PlayBGM("MainTheme");
    }
    #endregion


    #region UI 입력 이벤트들
    protected override void OnUIMove(Vector2 dir)
    {
        // 얘는 esc만 받기
    }

    public void UICancel()
    {
        OnUICancel();
    }
    protected override void OnUICancel()
    {
        //오프닝이라면 스킵
        if (isOpeningPhase)
        {
            ShowMenuImmediate();
            return;
        }
        if (mainExitPanel.cg.blocksRaycasts)
        {
            mainExitPanel.Close();
            mainPanel.enabled = true;
            return;
        }
        if (mainCharacterConfirmPanel.cg.blocksRaycasts)
        {
            mainCharacterConfirmPanel.Close();
            mainCharacterChoicePanel.enabled = true;
            return;
        }
        if (mainCharacterChoicePanel.cg.blocksRaycasts)
        {
            mainCharacterChoicePanel.Close();
            mainSaveDataPanel.enabled = true;
            return;
        }
        if (mainSaveDataPanel.cg.blocksRaycasts)
        {
            mainSaveDataPanel.Close();
            mainPanel.enabled = true;
            return;
        }
        mainPanel.enabled = false;
        mainExitPanel.Open();
    }
    #endregion

    public void OpenMainSaveDatePanel()
    {
        mainSaveDataPanel.Open();
        mainPanel.enabled = false;
    }
    public void Quit()
    {
        GameManager.Instance.QuitGame();
    }
}
using System.Linq;
using DG.Tweening;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartUI : UIKeyboardHandler
{
    [SerializeField] Image openingImage;

    [SerializeField] MainPanel mainPanel;
    [SerializeField] MainLoadSlotPanel mainLoadSlotPanel;
    [SerializeField] MainCharacterChoicePanel mainCharacterChoicePanel;
    [SerializeField] MainExitPanel mainExitPanel;

    DG.Tweening.Sequence openingSequence;

    bool isOpeningPhase = true;


    void Awake()
    {
        // 다른 UI들 숨긴 상태로 시작
        mainPanel.Close();
        mainLoadSlotPanel.Close();
        mainCharacterChoicePanel.Close();
        mainExitPanel.Close();
    }

    void Start()
    {
        // 오프닝은 보이게
        openingImage.gameObject.SetActive(true);
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
                .OnComplete(() =>
                {
                    ShowMenuImmediate();   // 최종 상태 정리 공용 함수
                })
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

    protected override void OnUICancel()
    {
        //오프닝이라면 스킵
        if (isOpeningPhase)
        {
            ShowMenuImmediate();
        }
        // 종료 하시겠습니까? UI 띄우기
        else
        {
            mainExitPanel.Open();
        }
    }
    #endregion

    public void Quit()
    {
        GameManager.Instance.QuitGame();
    }
}
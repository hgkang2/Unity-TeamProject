using System.Linq;
using DG.Tweening;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour,IUIKeyboardTarget
{
    [SerializeField] Image openingImage;

    [SerializeField] MainPanel mainPanel;
    [SerializeField] MainCharacterChoicePanel mainCharacterChoicePanel;
    [SerializeField] MainCharacterConfirmPanel mainCharacterConfirmPanel;


    Sequence openingSequence;

    bool isOpeningPhase = true;

    void Awake()
    {
        mainPanel.gameObject.SetActive(true);
        mainCharacterChoicePanel.gameObject.SetActive(true);
        mainCharacterConfirmPanel.gameObject.SetActive(true);
    }

    void Start()
    {
        // 다른 UI들 숨긴 상태로 시작
        mainPanel.Close();
        mainCharacterChoicePanel.Close();
        mainCharacterConfirmPanel.Close();

        // 오프닝은 보이게
        openingImage.gameObject.SetActive(true);

        InputManager.Instance.PushUI(this, true);
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

        openingSequence.OnComplete(() => { ShowMenuImmediate(); });
        // 3) 메뉴 fade-in (cg 직접 접근 못 하게 막아서 일단 패스)
        // openingSequence.Append(
        //     mainPanel.cg.DOFade(1f, 1f)
        //         .OnComplete(() => { ShowMenuImmediate(); })
        //);
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


        InputManager.Instance.PopUI(this, true);
        mainPanel.Open();

        SoundManager.Instance.PlayBGM("MainTheme");
    }
    #endregion


    #region UI 입력 이벤트들

    public void UICancel()
    {
        // 오프닝 중이라면 스킵
        if (isOpeningPhase) ShowMenuImmediate();
    }
    #endregion

    public void Quit()
    {
        GameManager.Instance.QuitGame();
    }

    public void OnUIInputMove(Vector2 dir)
    {
    }

    public void OnUIInputConfirm()
    {
        if (isOpeningPhase) ShowMenuImmediate();
    }

    public void OnUIInputCancel()
    {
        if (isOpeningPhase) ShowMenuImmediate();
    }
}
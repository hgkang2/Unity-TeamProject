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
    [SerializeField] MainCharacterChoicePanel mainCharacterChoicePanel;
    [SerializeField] ConfirmPanel mainCharacterConfirmPanel;


    Sequence openingSequence;

    // 게임 처음 시작 시에만 오프닝을 재생하는 플래그.
    static bool s_openingPlayedThisSession;
    bool isOpeningPhase = true;

    void Awake()
    {
        mainPanel.gameObject.SetActive(true);
        mainCharacterChoicePanel.gameObject.SetActive(true);
        mainCharacterConfirmPanel.gameObject.SetActive(true);
    }

    void Start()
    {
        mainPanel.Open();

        // 게임 시작시 1회만 오프닝 실행
        if (!s_openingPlayedThisSession)
        {
            s_openingPlayedThisSession = true;
            openingImage.gameObject.SetActive(true);
            isOpeningPhase = true;

            OpeningStart();
            InputManager.Instance.PushUI(this, true);
        }
        // 아니면 오프닝 실행 안 함
        else
        {
            openingImage.gameObject.SetActive(false);
            isOpeningPhase = false;
            ShowMenuImmediate();
        }
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
        if (!isOpeningPhase) return;

        if (openingSequence != null && openingSequence.IsActive())
            openingSequence.Kill();

        isOpeningPhase = false;
        openingImage.gameObject.SetActive(false);

        SoundManager.Instance.PlayBGM("MainTheme");
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
        ShowMenuImmediate();
        InputManager.Instance.PopUI(this, true);
    }

    public void OnUIInputCancel()
    {
        ShowMenuImmediate();
        InputManager.Instance.PopUI(this, true);
    }
}
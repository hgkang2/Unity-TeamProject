using System.Linq;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartPanel : UIKeyboardHandler
{
    [SerializeField] CanvasGroup openingGroup;
    [SerializeField] VideoPlayer openingPlayer;
    
    CanvasGroup startPanelCanvasGroup;

    bool isOpeningPhase = true;                         // 오프닝 중인지 여부
    Tween openingTimerTween;                            // DelayedCall 저장
    Sequence openingSequence;                           // 페이드용 시퀀스 저장


    Button[] menuButtons;
    [SerializeField] Transform ButtonSelectImage;
    [SerializeField] GameObject exitPanel;
    int? curIndex = null;

    void Awake()
    {
        startPanelCanvasGroup = GetComponent<CanvasGroup>();

        // 시작 시 메뉴는 숨기기
        startPanelCanvasGroup.alpha = 0f;
        startPanelCanvasGroup.interactable = false;
        startPanelCanvasGroup.blocksRaycasts = false;

        // 오프닝은 보이게
        openingGroup.gameObject.SetActive(true);
        openingGroup.alpha = 1f;
        openingGroup.interactable = false;
        openingGroup.blocksRaycasts = false;


        // 직계 자식 버튼들만 수집
        menuButtons = transform.Cast<Transform>()
            .Select(t => t.GetComponent<Button>())
            .Where(b => b != null)
            .ToArray();
    }

    void Start()
    {
        exitPanel.SetActive(false);
        ButtonSelectImage.gameObject.SetActive(false);
        UpdateHighlight();

        // 오프닝 자동 재생
        openingPlayer.prepareCompleted += OnVideoPrepared;
        openingPlayer.Prepare();
    }

    void OnDestroy()
    {
        openingPlayer.prepareCompleted -= OnVideoPrepared;
        openingSequence.Kill();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        if (!isOpeningPhase || vp.clip == null) return;

        // 영상 길이 가져오기
        float duration = (float)vp.clip.length;

        vp.Play();

        // 영상 길이만큼 기다렸다가 다음 단계 실행
        DOVirtual.DelayedCall(duration, OnOpeningFinished);
    }

    public void OnOpeningFinished()
    {
        if (!isOpeningPhase) return;

        openingSequence = DOTween.Sequence();

        // 1) Opening fade-out
        openingSequence.Append(
            openingGroup.DOFade(0f, 1)
        ).OnComplete(() =>
        {
            openingGroup.gameObject.SetActive(false);
        });

        // 2) 잠깐 대기
        openingSequence.AppendInterval(1);

        // 3) 메뉴 fade-in
        openingSequence.Append(
            startPanelCanvasGroup.DOFade(1f, 1f)
                .OnStart(() =>
                {
                    startPanelCanvasGroup.gameObject.SetActive(true);
                })
                .OnComplete(() =>
                {
                    ShowMenuImmediate();   // 최종 상태 정리 공용 함수
                })
        );
    }

    public void SkipOpening()
    {
        if (!isOpeningPhase) return;
        isOpeningPhase = false;

        // DOTween 타이머/시퀀스 모두 정리
        if (openingTimerTween != null && openingTimerTween.IsActive())
            openingTimerTween.Kill();

        if (openingSequence != null && openingSequence.IsActive())
            openingSequence.Kill();

        // 비디오도 정지
        openingPlayer.Stop();

        ShowMenuImmediate();
    }

    // 메뉴를 최종 상태로 만드는 공용 함수 (페이드 완료 or 스킵)
    void ShowMenuImmediate()
    {
        isOpeningPhase = false;

        if (openingGroup != null)
        {
            openingGroup.gameObject.SetActive(false);
            openingGroup.alpha = 0f;
        }

        startPanelCanvasGroup.gameObject.SetActive(true);
        startPanelCanvasGroup.alpha = 1f;
        startPanelCanvasGroup.interactable = true;
        startPanelCanvasGroup.blocksRaycasts = true;
    }

    protected override void OnUIMove(Vector2 dir)
    {
        if(isOpeningPhase) return; 

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
        if (curIndex == null)
        {
            ButtonSelectImage.gameObject.SetActive(false);
        }
        else
        {
            ButtonSelectImage.gameObject.SetActive(true);
            ButtonSelectImage.transform.position = menuButtons[(int)curIndex].transform.position;
        }
    }

    protected override void OnUICancel()
    {
        //오프닝이라면 스킵
        if (isOpeningPhase)
        {
            SkipOpening();
        }
        //종료확인창 이라면 우선 끄기
        else if (exitPanel.activeSelf == true)
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

    //버튼 선택만 해제
    public void QuitButtonSelect()
    {
        if (curIndex != null)
        {
            curIndex = null;
            UpdateHighlight();
        }
    }

    protected override void OnUIConfirm()
    {
        //선택 버튼이 없으면 무시
        if (curIndex == null) return;
        // 마우스로 클릭한 것과 동일하게 실행
        menuButtons[curIndex.Value].onClick.Invoke();
    }

    public void Quit()
    {
        GameManager.Instance.QuitGame();
    }
}

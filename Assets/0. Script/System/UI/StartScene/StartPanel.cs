using System.Linq;
using DG.Tweening;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartPanel : UIKeyboardHandler
{
    [SerializeField] Image openingImage;
    
    CanvasGroup startPanelCanvasGroup;

    DG.Tweening.Sequence openingSequence;

    bool isOpeningPhase = true;
    Button[] menuButtons;
    [SerializeField] Transform ButtonSelectImage;
    [SerializeField] GameObject exitPanel;

    void Awake()
    {
        startPanelCanvasGroup = GetComponent<CanvasGroup>();

        // 시작 시 메뉴는 숨기기
        startPanelCanvasGroup.alpha = 0f;
        startPanelCanvasGroup.interactable = false;
        startPanelCanvasGroup.blocksRaycasts = false;

        // 직계 자식 버튼들만 수집
        //  menuButtons = transform.Cast<Transform>()
        //      .Select(t => t.GetComponent<Button>())
        //      .Where(b => b != null)
        //      .ToArray();
        // activate가 비활성화된 버튼들은 수집 안하기
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
    }

    void Start()
    {
        // 오프닝은 보이게
        openingImage.gameObject.SetActive(true);
        exitPanel.SetActive(false);
        ButtonSelectImage.gameObject.SetActive(false);
        UpdateHighlight();
        OpeningStart();
    }

    void OnDestroy()
    {
        openingSequence.Kill();

        for (int i = 0; i < menuButtons.Length; i++)
        {
            StartButtonEvent evt = menuButtons[i].GetComponent<StartButtonEvent>();
            if (evt == null) continue;

            evt.onEnter -= ButtonMouseEnter;
            evt.onExit -= ButtonMouseExit;
        }
    }

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

        if (openingSequence != null && openingSequence.IsActive())
            openingSequence.Kill();

        ShowMenuImmediate();
    }

    // 메뉴를 최종 상태로 만드는 공용 함수 (페이드 완료 or 스킵)
    void ShowMenuImmediate()
    {
        isOpeningPhase = false;
        openingImage.gameObject.SetActive(false);

        startPanelCanvasGroup.gameObject.SetActive(true);
        startPanelCanvasGroup.alpha = 1f;
        startPanelCanvasGroup.interactable = true;
        startPanelCanvasGroup.blocksRaycasts = true;

        SoundManager.Instance.PlayBGM("MainTheme");
    }


    int? curIndex = null;
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
        if (curIndex < 0) curIndex = 0;
        else if (curIndex >= menuButtons.Length) curIndex = menuButtons.Length - 1;

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

    public void ButtonMouseEnter(int index)
    {
        curIndex = index;
        UpdateHighlight();
    }

    public void ButtonMouseExit()
    {
        ButtonSelectImage.gameObject.SetActive(false);
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class LevelUpPanel : UIKeyboardHandler
{
    [SerializeField] SoulManager soulManager;
    SoulPanel[] soulPanels;
    [SerializeField] Button rerollButton;
    [SerializeField] TMP_Text rerollText;
    [Header("리롤할 수 있는 횟수(매번 초기화됨)")]
    [SerializeField] int RerollNum = 2;
    int remainRerollNum;
    int panelNum = 2;
    SoulPanel selectedSoulPanel;

    public event Action SelectSoulCompleted;

    Vector3[] soulPanels_OriginPos = new Vector3[3];
    void Awake()
    {
        soulPanels = GetComponentsInChildren<SoulPanel>();
        for (int i = 0; i < 3; i++)
        {
            soulPanels_OriginPos[i] = soulPanels[i].transform.position;
        }
        SubscribeChildEvent();
        InputManager.Instance.UiRerolled += Reroll;
    }

    public void Initialize()
    {
        remainRerollNum = RerollNum + 1;
        rerollText.SetText("{0}", remainRerollNum);
        candidates = null;

        //대충 영성 선택지 2~3개 뜨게 하는 로직(임시)
        float rand = UnityEngine.Random.value;
        if (rand < 0.7) panelNum = 2;
        else panelNum = 3;

        for (int i = 0; i < 3; i++)
        {
            soulPanels[i].transform.position = soulPanels_OriginPos[i];
        }
        rerollButton.gameObject.SetActive(false);
        DrawSoul();
        StartAnim();
    }

    void OnDestroy()
    {
        UnSubscribeChildEvent();
        InputManager.Instance.UiRerolled -= Reroll;
    }

    #region 영성 anim 시작
    [Header("1. FadeIn : 시간 및 Ease")]
    [SerializeField] float first_FadeDuration = 0.4f;
    [SerializeField] Ease first_FadeInEase;

    [Header("2. 펼쳐짐 : 중심에서 카드까지의 거리")]
    [SerializeField] float radius = 200f;      // 중심에서 카드까지의 거리
    [Header("2. 펼쳐짐 : 전체 부채각")]
    [SerializeField] float angleRange = 30f;   // 전체 부채각 (예: 30도)

    [Header("3. FadeOut : 시간 및 Ease")]
    [SerializeField] float third_FadeDuration = 0.4f;
    [SerializeField] Ease third_FadeOutEase;

    [Header("4. FadeIn : 시간 및 Ease")]
    [SerializeField] float fifth_FadeDuration = 0.4f;
    [SerializeField] Ease fifth_FadeInEase;

    [Header("4. 내려오기 : 시간 및 Ease")]
    [SerializeField] float fifth_MoveDuration = 0.4f;
    [SerializeField] Ease fifth_MoveEase;

    [Header("5. 회전 : 시간 및 Ease")]
    [SerializeField] float sixth_MoveDuration = 0.4f;
    [SerializeField] Ease sixth_MoveEase;

    void StartAnim()
    {

        // 부채꼴 각도 세팅
        float startAngle = -angleRange * 0.5f;
        float step = (panelNum > 1) ? (angleRange / (panelNum - 1)) : 0f;
        float centerIndex = (panelNum - 1) * 0.5f;

        // 시퀀스 생성
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);


        // 연출 시작
        for (int i = 0; i < panelNum; i++)
        {
            soulPanels[i].transform.localPosition = Vector3.zero;
            soulPanels[i].gameObject.SetActive(true);
            soulPanels[i].InvisibleContent();

            CanvasGroup cg = soulPanels[i].GetComponent<CanvasGroup>();
            RectTransform rect = soulPanels[i].GetComponent<RectTransform>();

            // --- 초기 상태: 한 점에 겹쳐 있음 + 완전 투명 ---
            cg.alpha = 0f;
            DisableInput();

            rect.anchoredPosition = Vector3.zero;          // 전부 같은 위치


            Sequence cardSeq = DOTween.Sequence();

            // 1단계: Fade In
            cardSeq.Join(
                cg.DOFade(1f, first_FadeDuration)
                  .SetEase(first_FadeInEase)
            );

            // 2단계: 좌우로 살짝 퍼지기 + 회전 (V 느낌)
            float t = i - centerIndex;       // -1, 0, 1 같은 상대 인덱스
            float xOffset = radius * t;      // radius를 가로 간격처럼 사용

            Vector2 targetPos = new Vector2(Vector3.zero.x + xOffset, Vector3.zero.y);

            // 회전 각도: -angleRange/2 ~ +angleRange/2
            float angle = -(startAngle + step * i);

            // fadeDuration 이후에 위치/회전 트윈 시작
            cardSeq.Append(
                rect.DOAnchorPos(targetPos, 0.3f)
                    .SetEase(Ease.OutCubic)
            );

            float nowY = rect.localRotation.eulerAngles.y;
            cardSeq.Join(
                rect.DORotate(new Vector3(0f, nowY, angle), 0.3f, RotateMode.Fast)
                    .SetEase(Ease.OutCubic)
            );

            // 3단계 : Fade Out
            cardSeq.Append(
                cg.DOFade(0, third_FadeDuration)
                    .SetEase(third_FadeOutEase)
            );

            // 4단계 : 위치 재조정
            t = i - centerIndex;       // -1, 0, 1 같은 상대 인덱스
            targetPos = new Vector2(500 * t, 1000);

            cardSeq.Append(
                rect.DOAnchorPos(targetPos, 0.1f)
            );
            cardSeq.Join(
                rect.DORotate(new Vector3(0f, 0f, 0f), 0.1f, RotateMode.Fast)
            );

            //5단계 : Fade In + Down
            cardSeq.Append(
                cg.DOFade(1f, fifth_FadeDuration)
                  .SetEase(fifth_FadeInEase)
            );

            targetPos = new Vector2(500 * t, 0);
            cardSeq.Join(
                rect.DOAnchorPos(targetPos, fifth_MoveDuration)
                  .SetEase(fifth_MoveEase)
            );

            // 6단계 : 회전 + 내용 보이기
            cardSeq.Append(
                rect.DOLocalRotate(new Vector3(0f, 90f, 0f), sixth_MoveDuration / 2, RotateMode.LocalAxisAdd)
                    .SetEase(sixth_MoveEase)
            );

            int index = i;
            cardSeq.AppendCallback(() =>
            {
                soulPanels[index].visibleContent();
                rect.localRotation = Quaternion.Euler(0f, -90f, 0f);
            });

            cardSeq.Append(
                rect.DOLocalRotate(new Vector3(0f, 90f, 0f), sixth_MoveDuration / 2, RotateMode.LocalAxisAdd)
                    .SetEase(sixth_MoveEase)

            ).OnComplete(() =>
                {
                    rerollButton.gameObject.SetActive(true);
                    EnableInput();
                });

            seq.Join(cardSeq);
        }
    }
    #endregion

    SoulData[] candidates;
    public void Reroll()
    {
        if (remainRerollNum == 0) return;

        RerollButtonAnimation();

        DisableInput();

        // 시퀀스 생성
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        for (int i = 0; i < panelNum; i++)
        {
            RectTransform rect = soulPanels[i].GetComponent<RectTransform>();
            Sequence cardSeq = DOTween.Sequence();

            // 6단계 : 회전 + 내용 보이기
            cardSeq.Append(
                rect.DOLocalRotate(new Vector3(0f, 90f, 0f), sixth_MoveDuration / 2, RotateMode.LocalAxisAdd)
                    .SetEase(sixth_MoveEase)
            );

            int index = i;
            cardSeq.AppendCallback(() =>
            {
                //90도 돌아가서 안보이는 순간 내용 세팅하기
                if (index == 0) DrawSoul();
                //그냥 180도씩 돌리면 내용도 같이 돌려야 하는 등 귀찮아진다.
                rect.localRotation = Quaternion.Euler(0f, -90f, 0f);
            });

            cardSeq.Append(
                rect.DOLocalRotate(new Vector3(0f, 90f, 0f), sixth_MoveDuration / 2, RotateMode.LocalAxisAdd)
                    .SetEase(sixth_MoveEase)

            ).OnComplete(() => EnableInput());
            seq.Join(cardSeq);
        }

        remainRerollNum--;

        //남은 리롤 횟수에 따른 세팅
        //if (remainRerollNum == 0) rerollText.color = Color.red;
        //else rerollText.color = Color.white;
        rerollText.SetText("{0}", remainRerollNum);
        //if(remainRerollNum < RerollNum) PlayRerollIconAnim();
    }

    //리롤 버튼 애니메이션
    public void RerollButtonAnimation()
    {
        Sequence s;
        s = DOTween.Sequence();
        s.Append(rerollButton.transform.DOScale(0.95f, 0.25f).SetEase(Ease.OutQuint))
        .Append(rerollButton.transform.DOScale(1f, 0.25f).SetEase(Ease.InQuart))
        .SetUpdate(true);
    }

    //영성 뽑기(현재 활성화된 panel 만큼)
    void DrawSoul()
    {
        HandleDeSelectSoul();
        candidates = soulManager.GetSouls(candidates, panelNum);

        if (candidates == null) Debug.Log("뽑을 수 있는 영성이 없음");
        else
        {
            int i = 0;
            //반환된 소울 데이터의 길이 만큼 패널 세팅
            for (; i < candidates.Length; i++)
            {
                soulPanels[i].gameObject.SetActive(true);
                soulPanels[i].Set(candidates[i]);
            }
            //얻을 수 있는 소울이 부족하면 남는 패널은 끄기
            for (; i < soulPanels.Length; i++)
            {
                soulPanels[i].gameObject.SetActive(false);
            }
        }
    }
    
    //선택한 Soul을 등록
    public void EnrollSoul()
    {
        if (selectedSoulPanel == null) return;

        soulManager.EnrollSoul(selectedSoulPanel.SoulData);
        SelectSoulCompleted?.Invoke();
    }

    #region 마우스 입력 이벤트
    //SoulPanel에 마우스 올리면 커짐
    void HandleMouseHoverSoul(SoulPanel panel)
    {
        panel.ExpandPanelScale();
    }

    //SoulPanel에서 마우스 내리면 원래대로 작아짐
    void HandleMouseExitSoul(SoulPanel panel)
    {
        //단, 이미 선택한 Panel이면 안작아지고 그대로
        if (selectedSoulPanel == panel) return;
        panel.OriginPanelScale();
    }

    //SoulPanel을 클릭하면 해당 소울을 선택함
    void HandleClickSoul(SoulPanel panel)
    {
        //단, 이미 선택한 Panel이 있었으면 해당 Panel은 선택 취소
        if (selectedSoulPanel != null)
        {
            HandleDeSelectSoul();
        }

        selectedSoulPanel = panel;
        EnrollSoul();
    }

    //선택한 Panel 선택 취소
    public void HandleDeSelectSoul()
    {
        if (selectedSoulPanel == null) return;

        selectedSoulPanel.OriginPanelScale();
        selectedSoulPanel = null;
    }
    #endregion

    void SelectSoul(SoulPanel panel)
    {
        HandleDeSelectSoul();
        HandleMouseHoverSoul(panel);
        selectedSoulPanel = panel;
    }

    #region 이벤트 구독
    void SubscribeChildEvent()
    {
        UnSubscribeChildEvent();
        foreach (SoulPanel soulPanel in soulPanels)
        {
            soulPanel.SoulMouseEntered += HandleMouseHoverSoul;
            soulPanel.SoulMouseExited += HandleMouseExitSoul;
            soulPanel.SoulMouseClicked += HandleClickSoul;
        }
    }
    void UnSubscribeChildEvent()
    {
        foreach (SoulPanel soulPanel in soulPanels)
        {
            soulPanel.SoulMouseEntered -= HandleMouseHoverSoul;
            soulPanel.SoulMouseExited -= HandleMouseExitSoul;
            soulPanel.SoulMouseClicked -= HandleClickSoul;
        }
    }
    #endregion

    bool canInput = false;
    void EnableInput()
    {
        SubscribeChildEvent();
        rerollButton.GetComponent<Button>().enabled = true;
        canInput = true;
    }
    void DisableInput()
    {
        UnSubscribeChildEvent();
        rerollButton.GetComponent<Button>().enabled = false;
        canInput = false;
    }
    
    #region 키보드 조작
    //키보드로 영성 선택
    int? curIndex = null;
    protected override void OnUIMove(Vector2 dir)
    {
        if(!canInput) return;

        // 현재 아무것도 선택되지 않은 상태라면
        if (curIndex == null)
        {
            // 왼쪽/위쪽 → 0번 선택
            if (dir.x < -0.1f || dir.y > 0.1f) curIndex = 0;
            // 오른쪽/아래쪽 → 마지막 선택
            else if (dir.x > 0.1f || dir.y < -0.1f) curIndex = panelNum - 1;

            SelectSoul(soulPanels[(int)curIndex]);
            return;
        }

        //왼쪽 or 위쪽 방향키시 위쪽 방향으로
        if (dir.x < -0.1f || dir.y > 0.1) curIndex--;
        //오른쪽 or 아래쪽 방향키시 아래쪽 방향으로
        else if (dir.x > 0.1f || dir.y < -0.1) curIndex++;

        //min, max 처리
        if (curIndex < 0) curIndex = panelNum - 1;
        else if (curIndex >= panelNum) curIndex = 0;

        //강조된 버튼 변경
        SelectSoul(soulPanels[(int)curIndex]);
    }

    protected override void OnUIConfirm()
    {
        if(selectedSoulPanel == null) return;
        EnrollSoul();
    }

    protected override void OnUICancel()
    {
        curIndex = null;
        HandleDeSelectSoul();
    }
    #endregion
}


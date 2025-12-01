using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using Unity.Mathematics;

public class LevelUpPanel : MonoBehaviour
{
    [SerializeField] SoulManager soulManager;
    SoulPanel[] soulPanels;
    [SerializeField] Transform rerollIcon;
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
        DrawSoul();
        StartAnim();
    }

    void OnEnable()
    {
        SubscribeChildEvent();
    }
    void OnDisable()
    {
        UnSubscribeChildEvent();
    }

    void Update()
    {
        //좌우 방향키로 영성 선택 기능
        // if (Input.GetKeyDown(KeyCode.LeftArrow))
        // {
        //     if (selectIndex == -1)
        //     {
        //         HandleSelectSoul(soulPanels[0]);
        //     }
        //     else
        //     {
        //         //selectIndex - 1이 0보다 작으면 0으로 만들기
        //         selectIndex = --selectIndex < 0 ? 0 : selectIndex;
        //         HandleSelectSoul(soulPanels[selectIndex]);
        //     }
        // }
        // else if (Input.GetKeyDown(KeyCode.RightArrow))
        // {
        //     int max = soulPanels.Length - 1;
        //     if (selectIndex == -1)
        //     {
        //         HandleSelectSoul(soulPanels[max]);
        //     }
        //     else
        //     {
        //         //selectIndex + 1이 length-1보다 크면 length-1으로 만들기
        //         selectIndex = ++selectIndex > max ? max : selectIndex;
        //         HandleSelectSoul(soulPanels[selectIndex]);
        //     }
        // }
        // else if (Input.GetKeyDown(KeyCode.D))
        // {
        //     Reroll();
        // }
        // else if (Input.GetKeyDown(KeyCode.F))
        // {
        //     EnrollSoul();
        // }
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
        // 기준이 되는 중심 위치 (첫 패널 위치를 기준으로)
        RectTransform centerRect = soulPanels[1].GetComponent<RectTransform>();
        Vector2 centerPos = centerRect.anchoredPosition;

        // 부채꼴 각도 세팅
        float startAngle = -angleRange * 0.5f;
        float step = (panelNum > 1) ? (angleRange / (panelNum - 1)) : 0f;
        float centerIndex = (panelNum - 1) * 0.5f;

        //모두 끄고 시작
        foreach (var panel in soulPanels)
        {
            panel.gameObject.SetActive(false);
        }

        // 시퀀스 생성
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);


        // 연출 시작
        for (int i = 0; i < panelNum; i++)
        {
            soulPanels[i].gameObject.SetActive(true);
            soulPanels[i].InvisibleContent();

            CanvasGroup cg = soulPanels[i].GetComponent<CanvasGroup>();
            RectTransform rect = soulPanels[i].GetComponent<RectTransform>();

            // --- 초기 상태: 한 점에 겹쳐 있음 + 완전 투명 ---
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            rect.anchoredPosition = centerPos;          // 전부 같은 위치


            Sequence cardSeq = DOTween.Sequence();

            // 1단계: Fade In
            cardSeq.Join(
                cg.DOFade(1f, first_FadeDuration)
                  .SetEase(first_FadeInEase)
            );

            // 2단계: 좌우로 살짝 퍼지기 + 회전 (V 느낌)
            float t = i - centerIndex;       // -1, 0, 1 같은 상대 인덱스
            float xOffset = radius * t;      // radius를 가로 간격처럼 사용

            Vector2 targetPos = new Vector2(centerPos.x + xOffset, centerPos.y);

            // 회전 각도: -angleRange/2 ~ +angleRange/2
            float angle = -(startAngle + step * i);

            // fadeDuration 이후에 위치/회전 트윈 시작
            cardSeq.Append(
                rect.DOAnchorPos(targetPos, 0.5f)
                    .SetEase(Ease.OutCubic)
            );

            float nowY = rect.localRotation.eulerAngles.y;
            cardSeq.Join(
                rect.DORotate(new Vector3(0f, nowY, angle), 0.5f, RotateMode.Fast)
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
                //카드가 뒤집힌 상태면 내용물도 같이 뒤집기
                float yRot = rect.localRotation.eulerAngles.y % 360f;

                // 90° 또는 270° 부근일 때 뒤집기 적용
                if (Mathf.Abs(yRot - 90f) < 1f)
                {
                    soulPanels[index].FlipContent();
                }
                else if (Mathf.Abs(yRot - 270f) < 1f)
                {
                    soulPanels[index].NoFlipContent();
                }
            });

            cardSeq.Append(
                rect.DOLocalRotate(new Vector3(0f, 90f, 0f), sixth_MoveDuration / 2, RotateMode.LocalAxisAdd)
                    .SetEase(sixth_MoveEase)

            ).OnComplete(() =>
                {
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                });

            seq.Join(cardSeq);
        }
    }
    #endregion

    SoulData[] candidates;
    public void Reroll()
    {
        if (remainRerollNum == 0) return;

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
                DrawSoul();
                //카드가 뒤집힌 상태면 내용물도 같이 뒤집기
                float yRot = rect.localRotation.eulerAngles.y % 360f;

                // 90° 또는 270° 부근일 때 뒤집기 적용
                if (Mathf.Abs(yRot - 90f) < 1f)
                {
                    soulPanels[index].FlipContent();
                }
                else if (Mathf.Abs(yRot - 270f) < 1f)
                {
                    soulPanels[index].NoFlipContent();
                }
            });

            cardSeq.Append(
                rect.DOLocalRotate(new Vector3(0f, 90f, 0f), sixth_MoveDuration / 2, RotateMode.LocalAxisAdd)
                    .SetEase(sixth_MoveEase)

            );
            seq.Join(cardSeq);
        }

        remainRerollNum--;

        //남은 리롤 횟수에 따른 세팅
        //if (remainRerollNum == 0) rerollText.color = Color.red;
        //else rerollText.color = Color.white;
        rerollText.SetText("{0}", remainRerollNum);
        //if(remainRerollNum < RerollNum) PlayRerollIconAnim();
    }

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

    //SoulPanel에 마우스 올리면 커짐
    void HandleMouseHoverSoul(SoulPanel panel)
    {
        //단, 이미 선택한 Panel이 있으면 취소
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
    void HandleSelectSoul(SoulPanel panel)
    {
        //단, 이미 선택한 Panel이 있었으면 해당 Panel은 선택 취소
        if (selectedSoulPanel != null)
        {
            HandleDeSelectSoul();
        }

        selectedSoulPanel = panel;
        //이전에 선택된 Panel 때문에 못 커졌으니 지금이라도 커지게 함
        selectedSoulPanel.ExpandPanelScale();
    }

    //선택한 Panel 선택 취소
    public void HandleDeSelectSoul()
    {
        if (selectedSoulPanel == null) return;

        selectedSoulPanel.OriginPanelScale();
        selectedSoulPanel = null;
    }

    //선택한 Soul을 등록
    public void EnrollSoul()
    {
        if (selectedSoulPanel == null) return;

        soulManager.EnrollSoul(selectedSoulPanel.SoulData);
        SelectSoulCompleted?.Invoke();
    }

    #region 이벤트 구독
    void SubscribeChildEvent()
    {
        UnSubscribeChildEvent();
        foreach (SoulPanel soulPanel in soulPanels)
        {
            soulPanel.SoulMouseEntered += HandleMouseHoverSoul;
            soulPanel.SoulMouseExited += HandleMouseExitSoul;
            soulPanel.SoulMouseClicked += HandleSelectSoul;
        }
    }
    void UnSubscribeChildEvent()
    {
        foreach (SoulPanel soulPanel in soulPanels)
        {
            soulPanel.SoulMouseEntered -= HandleMouseHoverSoul;
            soulPanel.SoulMouseExited -= HandleMouseExitSoul;
            soulPanel.SoulMouseClicked -= HandleSelectSoul;
        }
    }
    #endregion


    #region Dotween animation
    //리롤 아이콘 딸깍 하는 애니메이션
    int nowrot = 0;
    public void PlayRerollIconAnim()
    {
        // rerollIcon.DOKill();

        // Sequence seq = DOTween.Sequence().SetUpdate(true); ;

        // seq.Append(rerollIcon.DOScale(originalScale * 0.95f, duration * 0.6f))   // 1. 살짝 줄어듦

        //    .Join(rerollIcon.DORotate(new Vector3(0, 0, nowrot + -120f), duration * 0.6f) // 2. 오른쪽으로 크게 회전
        //         .SetEase(Ease.OutBack));

        //    //.Append(rerollIcon.DOScale(originalScale, duration * .4f));          // 3. 원래 크기로 복귀

        //    //.Join(rerollIcon.DORotate(new Vector3(0, 0, nowrot + 10f), duration * 1f) // 4. 살짝 덜 돌아온 상태로 복귀
        //    //     .SetEase(Ease.OutBack));
        //     nowrot -= 120;
    }

    #endregion
}


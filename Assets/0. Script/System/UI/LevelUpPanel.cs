using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEditor.PackageManager.Requests;
using DG.Tweening;

public class LevelUpPanel : MonoBehaviour
{
    [SerializeField] SoulManager soulManager;
    SoulPanel[] soulPanels;
    int selectIndex = -1;
    [SerializeField] Transform rerollIcon;
    [SerializeField] TMP_Text rerollText;
    [SerializeField] int RerollNum = 2;
    int remainRerollNum;
    SoulPanel selectedSoulPanel;

    public event Action SelectSoulCompleted;

    //======리롤 아이콘 테스트 용========
    [SerializeField] float duration = 0.15f; // 눌림-복귀까지 총 시간
    Vector3 originalScale;
    Vector3 originalRotation;
    //======리롤 아이콘 테스트 용========

    void Awake()
    {
        soulPanels = GetComponentsInChildren<SoulPanel>();
        //======리롤 아이콘 테스트 용========
        originalScale = rerollIcon.localScale;
        originalRotation = rerollIcon.localEulerAngles;
        //======리롤 아이콘 테스트 용========
    }

    public void Initialize()
    {
        remainRerollNum = RerollNum+1;
        candidates = null;
    }

    void OnEnable()
    {
        SubscribeChildEvent();
        Initialize();
    }
    void OnDisable()
    {
        UnSubscribeChildEvent();
    }

    void Update()
    {
        //좌우 방향키로 영성 선택 기능
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectIndex == -1)
            {
                HandleSelectSoul(soulPanels[0]);
            }
            else
            {
                //selectIndex - 1이 0보다 작으면 0으로 만들기
                selectIndex = --selectIndex < 0 ? 0 : selectIndex;
                HandleSelectSoul(soulPanels[selectIndex]);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int max = soulPanels.Length - 1;
            if (selectIndex == -1)
            {
                HandleSelectSoul(soulPanels[max]);
            }
            else
            {
                //selectIndex + 1이 length-1보다 크면 length-1으로 만들기
                selectIndex = ++selectIndex > max ? max : selectIndex;
                HandleSelectSoul(soulPanels[selectIndex]);
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Reroll();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            EnrollSoul();
        }
    }

    SoulData[] candidates;
    public void Reroll()
    {
        if (remainRerollNum == 0) return;
        
        selectIndex = -1;
        HandleDeSelectSoul();

        candidates = soulManager.GetSouls(candidates, soulPanels.Length);

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

        remainRerollNum--;

        //남은 리롤 횟수에 따른 세팅
        //if (remainRerollNum == 0) rerollText.color = Color.red;
        //else rerollText.color = Color.white;
        rerollText.SetText("{0}", remainRerollNum);
        if(remainRerollNum < RerollNum) PlayRerollIconAnim();
    }

    //SoulPanel에 마우스 올리면 커짐
    void HandleMouseHoverSoul(SoulPanel panel)
    {
        //단, 이미 선택한 Panel이 있으면 취소
        if (selectedSoulPanel != null) return;
        panel.ExpandPanelScale();
    }

    //SoulPanel에서 마우스 내리면 원래대로 작아짐
    void HandleMouseExitSoul(SoulPanel panel)
    {
        //단, 이미 선택한 Panel이 있으면 취소
        if (selectedSoulPanel != null) return;
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

    //선택한 Soul을 플레이어에게 등록
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
        return;
        rerollIcon.DOKill();

        Sequence seq = DOTween.Sequence().SetUpdate(true); ;

        seq.Append(rerollIcon.DOScale(originalScale * 0.95f, duration * 0.6f))   // 1. 살짝 줄어듦
            
           .Join(rerollIcon.DORotate(new Vector3(0, 0, nowrot + -120f), duration * 0.6f) // 2. 오른쪽으로 크게 회전
                .SetEase(Ease.OutBack));

           //.Append(rerollIcon.DOScale(originalScale, duration * .4f));          // 3. 원래 크기로 복귀

           //.Join(rerollIcon.DORotate(new Vector3(0, 0, nowrot + 10f), duration * 1f) // 4. 살짝 덜 돌아온 상태로 복귀
           //     .SetEase(Ease.OutBack));
            nowrot -= 120;
    }

    #endregion
}


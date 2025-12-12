using UnityEngine;
using System.Collections.Generic;

public class HaveSoulsUI : MonoBehaviour
{
    [SerializeField] HaveSoulTooltipUI tooltipUI;
    [SerializeField] SoulManager SM;
    [SerializeField] HaveSoulSlot haveSoulSlotPrefab;
    List<HaveSoulUI> soulUIs = new List<HaveSoulUI>();
    
    void Awake()
    {
        HideTooltipUI();
    }

    void OnEnable()
    {
        HideTooltipUI();
        
        //이름순 정렬하기
        //foreach (SoulData soul in stats.Souls.OrderBy(soul => soul.soulName)) 
        //단순 획득순 정렬
        foreach (SoulInstance soul in SM.CurSouls)
        {
            HaveSoulUI haveSoulUI = Instantiate(prefabHaveSoulUI, transform);
            haveSoulUI.MouseEntered += ShowTooltipUI;
            haveSoulUI.MouseExited += HideTooltipUI;
            haveSoulUI.Bind(soul);
            soulUIs.Add(haveSoulUI);
        }
    }
    
    void OnDisable()
    {
        foreach (HaveSoulUI haveSoulUI in soulUIs)
        {
            haveSoulUI.MouseEntered -= ShowTooltipUI;
            haveSoulUI.MouseExited -= HideTooltipUI;
            Destroy(haveSoulUI.gameObject);
        }
        soulUIs.Clear();
        HideTooltipUI();
    }

    void ShowTooltipUI(RectTransform slotRect, SoulInstance inst)
    {
        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipUI.rect.parent as RectTransform,
            corners[2],
            null,
            out local);

        tooltipUI.rect.anchoredPosition = local;

        tooltipUI.gameObject.SetActive(true);
        tooltipUI.Set(inst.data);
    }

    void HideTooltipUI()
    {
        tooltipUI.gameObject.SetActive(false);
    }
}


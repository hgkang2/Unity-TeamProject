using UnityEngine;
using System.Collections.Generic;
using RuntimeInspectorNamespace;

public class HaveSoulsPanel : MonoBehaviour
{
    [SerializeField] HaveSoulTooltipUI tooltipUI;
    [SerializeField] SoulManager SM;
    [SerializeField] HaveSoulSlot haveSoulSlotPrefab;
    List<IInteractiveView<SoulData>> uiSlots = new List<IInteractiveView<SoulData>>();
    [SerializeField] List<Transform> emptySlot = new List<Transform>();

    SoulPanelEventAggregator forwarder;
    
    
    void Awake()
    {
        forwarder = GetComponent<SoulPanelEventAggregator>();
        HideTooltipUI();
    }

    void OnEnable()
    {
        HideTooltipUI();

        forwarder.MouseEntered += HandleMouseEnter;
        forwarder.MouseExited += HandleMouseExit;

        // 슬롯 생성 및 바인딩
        for(int i=0; i<SM.CurSouls.Count; i++)
        {
            HaveSoulSlot haveSoulUI = Instantiate(haveSoulSlotPrefab, transform);
            haveSoulUI.Bind(SM.CurSouls[i].data);
            uiSlots.Add(haveSoulUI);

            haveSoulUI.transform.position = emptySlot[i].transform.position;

        }

        // 이벤트 구독 재설정
        forwarder.RebuildViews();
    }

    void OnDisable()
    {
        forwarder.MouseEntered -= HandleMouseEnter;
        forwarder.MouseExited -= HandleMouseExit;

        foreach (var slot in uiSlots)
            Destroy(slot.GO);

        uiSlots.Clear();
        HideTooltipUI();
    }

    void HandleMouseEnter(SlotEventArgs<SoulData> e)
    {
        ShowTooltipUI( e.Data);
    }

    void HandleMouseExit(SlotEventArgs<SoulData> e)
    {
        HideTooltipUI();
    }

    void ShowTooltipUI(SoulData data)
    {
        tooltipUI.Show(data);
    }

    void HideTooltipUI()
    {
        tooltipUI.Hide();
    }
}


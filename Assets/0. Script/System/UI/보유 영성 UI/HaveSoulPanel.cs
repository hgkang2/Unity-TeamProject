using UnityEngine;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using TMPro;
using UnityEngine.UIElements;

public class HaveSoulsPanel : MonoBehaviour
{
    [SerializeField] HaveSoulTooltipUI soulTooltipUI;
    [SerializeField] HaveSoulSlot haveSoulSlotPrefab;
    List<IInteractiveView<SoulData>> uiSlots = new List<IInteractiveView<SoulData>>();
    [SerializeField] List<Transform> emptySlot = new List<Transform>();
    SoulPanelEventAggregator forwarder;

    [SerializeField] TMP_Text soulLevel;
    [SerializeField] TMP_Text soulExp;
    [SerializeField] List<TMP_Text> soulEffects;

    [SerializeField] Exp exp;
    
    
    void Awake()
    {
        forwarder = GetComponent<SoulPanelEventAggregator>();
        exp = FindFirstObjectByType<Exp>();
    }

    void OnEnable()
    {
        HideTooltipUI();

        UpdateExp(exp.CurExp, exp.MaxExp);
        UpdateLevel(exp.CurLevel);

        if (SoulManager.Instance != null)
        {
            UpdateSoulEffects(SoulManager.Instance.CurSouls);
            SoulManager.Instance.soulGot += UpdateSoulEffects;
        }

        // 게임 데이터 구독
        exp.ExpChanged += UpdateExp;
        exp.LevelChanged += UpdateLevel;
        
        // UI 마우스 이벤트 구독
        forwarder.MouseEntered += HandleMouseEnter;

        // 미니아이콘 슬롯 생성 및 바인딩
        for(int i=0; i<SoulManager.Instance.CurSouls.Count; i++)
        {
            HaveSoulSlot haveSoulUI = Instantiate(haveSoulSlotPrefab, transform);
            haveSoulUI.Bind(SoulManager.Instance.CurSouls[i].data);
            uiSlots.Add(haveSoulUI);

            haveSoulUI.transform.position = emptySlot[i].transform.position;

        }

        // 이벤트 구독 재설정
        forwarder.RebuildViews();
    }

    void OnDisable()
    {
        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.soulGot -= UpdateSoulEffects;
        }
        forwarder.MouseEntered -= HandleMouseEnter;
        foreach (var slot in uiSlots)
            Destroy(slot.GO);

        uiSlots.Clear();
        HideTooltipUI();
    }
    void UpdateExp(int newCurExp, int newMaxExp)
    {
        soulExp.SetText($"{newCurExp} / {newMaxExp}");
    }

    void UpdateLevel(int newCurLevel)
    {
        soulLevel.SetText($"LV. {newCurLevel}");
    }

    [SerializeField] TMP_Text statText;
    [SerializeField] TMP_Text effectText;
    void UpdateSoulEffects(List<SoulInstance> curSouls)
    {
        if(curSouls == null || curSouls.Count == 0) return;
        statText.SetText("");
        effectText.SetText("");
        foreach(SoulInstance soul in curSouls)
        {
            SoulEffectType type = soul.data.effect.type;
            if(type == SoulEffectType.StatFlat || type == SoulEffectType.StatPercent)
            {
                statText.text += $"{soul.GetEffectText()}\n";
            }
            else
            {
                effectText.text += $"{soul.GetEffectText()}\n";
            }
        }
    }


    void HandleMouseEnter(SlotEventArgs<SoulData> e)
    {
        ShowTooltipUI(e.Data);
    }

    // 슬롯이 아닌 슬롯 패널(MouseEventSupporter) 밖으로 나가야 선택 취소되고 뒷면 보여짐
    public void SlotMouseExit()
    {
        HideTooltipUI();
    }

    void HandleMouseClick(SlotEventArgs<SoulData> e)
    {
        //클릭했을 때 고정되는 등 효ㅘ
        //HideTooltipUI();
    }

    void ShowTooltipUI(SoulData data)
    {
        soulTooltipUI.Show(data);
    }

    void HideTooltipUI()
    {
        soulTooltipUI.Hide();
    }
}


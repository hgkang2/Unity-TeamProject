using UnityEngine;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using TMPro;
using UnityEngine.UIElements;

public class HaveSoulsPanel : UIPanelBase
{
    SceneContext sceneContext;

    // 좌측 패널
    [SerializeField] Transform statSlotPanel;
    [SerializeField] Transform beadPanel;

    //중간 패널
    [SerializeField] Transform[] soulSlots; 

    //우측 패널
    [SerializeField] HaveSoulTooltipUI soulTooltipUI;

    Exp exp;


    protected override void Init()
    {
        sceneContext = FindFirstObjectByType<SceneContext>();
        exp = sceneContext.player.Exp;
    }
    protected override void OnOpened()
    {
        HideTooltipUI();

        UpdateSoulEffects(SoulManager.Instance.CurSouls);
        SoulManager.Instance.soulGot += UpdateSoulEffects;
    }

    protected override void OnClosing()
    {
        SoulManager.Instance.soulGot -= UpdateSoulEffects;
        HideTooltipUI();
    }


    [SerializeField] TMP_Text statText;
    [SerializeField] TMP_Text effectText;

    void UpdateSoulEffects(List<SoulInstance> curSouls)
    {
        if (curSouls == null || curSouls.Count == 0) return;
        statText.SetText("");
        effectText.SetText("");
        foreach (SoulInstance soul in curSouls)
        {
            SoulEffectType type = soul.data.effect.type;
            if (type == SoulEffectType.StatFlat || type == SoulEffectType.StatPercent)
            {
                statText.text += $"{soul.GetEffectText()}\n";
            }
            else
            {
                effectText.text += $"{soul.GetEffectText()}\n";
            }
        }
    }
    public void ShowTooltipUI(SoulData data)
    {
        soulTooltipUI.Show(data);
    }

    public void HideTooltipUI()
    {
        soulTooltipUI.Hide();
    }
}


using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HaveSoulPanel : UIPanelBase
{
    SceneContext sceneContext;

    // 좌측 패널
    [SerializeField] HaveStatPanel statPanel;
    [SerializeField] HaveBeadPanel beadPanel;

    //중간 패널
    [SerializeField] HaveSoulSlot[] soulSlots; 

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

        Refresh(SoulManager.Instance.CurSouls);
        SoulManager.Instance.soulGot += Refresh;
    }

    protected override void OnClosing()
    {
        SoulManager.Instance.soulGot -= Refresh;
    }

    void Refresh(List<SoulInstance> souls)
    {
        var onlySouls = 
            souls.Where(s => s.data.soulType == SoulType.Soul);
        var onlyBeads = 
            souls.Where(s => s.data.soulType == SoulType.Bead);
        
        // 중앙 영성 슬롯 채우기
        int i = 0;
        foreach(var item in onlySouls)
        {
            soulSlots[i++].Set(item);
        }

        beadPanel.Set(onlyBeads);
        
        foreach(var item in onlySouls)
        {
            statPanel.AddStat(item);
        }
        foreach(var item in onlyBeads)
        {
            statPanel.AddStat(item);
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


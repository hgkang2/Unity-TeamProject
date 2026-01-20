using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HaveBeadSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    HaveSoulPanel haveSoulPanel;
    SoulData soulData;
    public SoulData SoulData => soulData;
    [SerializeField] Image beadImage;
    [SerializeField] TMP_Text numText;
    CanvasGroup cg;
    void Awake()
    {
        haveSoulPanel = GetComponentInParent<HaveSoulPanel>();
        cg = GetComponent<CanvasGroup>();
    }

    public void PreSet(SoulData data)
    {
        soulData = data;
        beadImage.sprite = soulData.soulIcon;
    }
    public void Set(SoulInstance instance)
    {
        if(instance.data.maxStack <= 1) numText.SetText($"고유");
        else numText.SetText($"{instance.stack}");
    }

    public void Show()
    {
        cg.alpha = 1;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    public void Hide()
    {
        cg.alpha = 0;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (soulData == null) return;
        haveSoulPanel.ShowTooltipUI(soulData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (soulData == null) return;
        haveSoulPanel.HideTooltipUI();
    }
}

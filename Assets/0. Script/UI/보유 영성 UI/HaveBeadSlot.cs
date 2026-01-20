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
    void Awake()
    {
        haveSoulPanel = GetComponentInParent<HaveSoulPanel>();
    }

    public void PreSet(SoulData data)
    {
        soulData = data;
        beadImage.sprite = soulData.soulIcon;
    }
    public void Set(SoulInstance instance)
    {
        numText.SetText($"{instance.stack}");
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

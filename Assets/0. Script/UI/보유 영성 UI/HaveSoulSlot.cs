using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HaveSoulSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    HaveSoulPanel haveSoulPanel;
    [SerializeField] Image soulImage;
    SoulInstance instance;

    void Awake()
    {
        haveSoulPanel = GetComponentInParent<HaveSoulPanel>();
        soulImage.gameObject.SetActive(false);
    }

    public void Set(SoulInstance item)
    {
        instance = item;
        soulImage.sprite = instance.data.soulIcon;
        soulImage.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(instance == null || instance.data == null) return;
        haveSoulPanel.ShowTooltipUI(instance.data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(instance == null || instance.data == null) return;
        haveSoulPanel.HideTooltipUI();
    }
}

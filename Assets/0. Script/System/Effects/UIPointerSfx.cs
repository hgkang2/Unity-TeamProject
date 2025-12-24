using UnityEngine;
using UnityEngine.EventSystems;

public class UIPointerSfx : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler
{
    [Header("UI Sound Keys")]
    [SerializeField] string hoverKey = "Button_Hover";
    [SerializeField] string clickKey = "Button_Click";

    [Header("Options")]
    [SerializeField] bool enableHover = true;
    [SerializeField] bool enableClick = true;
    bool hovered;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableHover) return;
        if (string.IsNullOrEmpty(hoverKey)) return;
        if (hovered) return;
        hovered = true;
        SoundManager.Instance.PlayUI(hoverKey);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!enableClick) return;
        if (string.IsNullOrEmpty(clickKey)) return;

        SoundManager.Instance.PlayUI(clickKey);
    }
}

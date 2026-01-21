using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ConfirmPanel 전용 hover relay
/// - PointerEnter / Down : 해당 버튼으로 포커스 이동
/// - PointerExit        : 패널의 기본(키보드) 포커스로 복귀
/// </summary>
public class ConfirmButtonHoverRelay 
    : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
{
    ConfirmPanel panel;
    Button button;

    void Awake()
    {
        panel = GetComponentInParent<ConfirmPanel>();
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        panel.SetCurrent(button);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        panel.SetCurrent(button);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        panel.SetCurrent(null);
    }
}

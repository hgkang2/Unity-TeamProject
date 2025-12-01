using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class StartButtonEvent : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent onEnter;
    public UnityEvent onExit;
    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onExit?.Invoke();
    }

}
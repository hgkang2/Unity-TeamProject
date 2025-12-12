using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIPointerHandler : MonoBehaviour
{
}

public abstract class UIPointerHandler<T> : UIPointerHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Func<T> GetData;
    public Func<RectTransform> GetRect;

    // 필요하면 PointerEventData도 같이 넘길 수 있게 확장
    public event Action<T, RectTransform> PointerEntered;
    public event Action PointerExited;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (GetData == null) return;

        T data = GetData();
        if (data == null) return;

        if (GetRect == null) return;
        RectTransform rect = GetRect();
        if (rect == null) return;

        PointerEntered?.Invoke(data, rect);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        PointerExited?.Invoke();
    }
}

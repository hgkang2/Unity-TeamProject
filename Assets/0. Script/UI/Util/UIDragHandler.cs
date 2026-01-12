using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIDragHandler : MonoBehaviour { }

public class UIDragHandler<T> : UIDragHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Func<T> GetData;

    public event Action<T, PointerEventData> DragBegan;
    public event Action<PointerEventData> Dragging;
    public event Action<T, PointerEventData> DragEnded;

    public Action SetGhostInvisible;
    public Action SetGhostVisible;

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (GetData == null) return;
        T data = GetData();
        if (data == null) return;

        SetGhostInvisible?.Invoke();
        DragBegan?.Invoke(data, eventData);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Dragging?.Invoke(eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (GetData == null)
        {
            SetGhostVisible?.Invoke();
            return;
        }

        T data = GetData();
        SetGhostVisible?.Invoke();
        DragEnded?.Invoke(data, eventData);
    }
}

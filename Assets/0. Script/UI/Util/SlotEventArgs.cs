using UnityEngine;
using UnityEngine.EventSystems;

public readonly struct SlotEventArgs<T>
{
    public SlotEventArgs(T data, RectTransform rect, PointerEventData pointer)
    {
        Data = data;
        Rect = rect;
        Pointer = pointer;
    }

    public T Data { get; }
    public RectTransform Rect { get; }
    public PointerEventData Pointer { get; }
}

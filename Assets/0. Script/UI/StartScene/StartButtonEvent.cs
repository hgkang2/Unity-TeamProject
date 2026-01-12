using System;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class StartButtonEvent : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public bool activate;
    public int myIndex;
    public event Action<int> onEnter;
    public event Action onExit;

    public void SetIndex(int idx)
    {
        myIndex = idx;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!activate) return;
        onEnter?.Invoke(myIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!activate) return;
        onExit?.Invoke();
    }
}
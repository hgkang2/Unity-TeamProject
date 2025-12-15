using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIClickHandler : MonoBehaviour { }

public class UIClickHandler<T> : UIClickHandler, IPointerClickHandler
{
    public Func<T> GetData;

    public event Action<T> LeftClicked;
    public event Action<T> RightClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GetData == null) return;

        T data = GetData();
        if (data == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeftClicked?.Invoke(data);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClicked?.Invoke(data);
        }
    }
}

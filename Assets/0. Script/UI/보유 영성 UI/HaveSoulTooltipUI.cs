using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HaveSoulTooltipUI : MonoBehaviour
{
    public RectTransform rect;
    
    
    [SerializeField] Image soulImage; // front랑 같음
    [SerializeField] CanvasGroup front;
    [SerializeField] CanvasGroup back;



    public void Show(SoulData data)
    {
        Set(data);
        front.alpha = 1;
        back.alpha = 0;
    }

    public void Hide()
    {
        front.alpha = 0;
        back.alpha = 1;
    }

    public void Set(SoulData data)
    {
        soulImage.sprite = data.soulSprite;
    }
}

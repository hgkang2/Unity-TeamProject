using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HaveSoulTooltipUI : MonoBehaviour
{
    public RectTransform rect;
    
    [SerializeField] CanvasGroup front;
    [SerializeField] CanvasGroup back;
    [SerializeField] Image soulImage;
    [SerializeField] TMP_Text soulName;
    [SerializeField] TMP_Text soulEffect;
    [SerializeField] TMP_Text soulDescript;



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
        soulName.text = data.displayName;
        soulEffect.text = data.soulEffectText;
        soulDescript.text = data.soulDescript;
    }
}

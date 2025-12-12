using UnityEngine;
using TMPro;

public class HaveSoulTooltipUI : MonoBehaviour
{
    public RectTransform rect;
    [SerializeField] TMP_Text soulDescript;
    
    public void Set(SoulData data){
        soulDescript.text = data.soulDescript;
    }
}

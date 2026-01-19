using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HaveBeadSlot : MonoBehaviour
{
    SoulData soulData;
    public SoulData SoulData => soulData;
    [SerializeField] Image beadImage;
    [SerializeField] TMP_Text numText;

    public void PreSet(SoulData data)
    {
        soulData = data;
        beadImage.sprite = soulData.soulIcon;
    }
    public void Set(SoulInstance instance)
    {
        numText.SetText($"{instance.stack}");
    }
}

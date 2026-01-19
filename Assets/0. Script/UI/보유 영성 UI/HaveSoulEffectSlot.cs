using TMPro;
using UnityEngine;

public class HaveSoulEffectSlot : MonoBehaviour
{
    TMP_Text text;

    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    public void Set(SoulInstance instance)
    {
        // ex) {ATK +} {5 *stack} {%}
        text.SetText($"{instance.data.soulEffectText}{instance.data.GetValue() * instance.stack}{instance.data.soulEffectText2}");
    }
}

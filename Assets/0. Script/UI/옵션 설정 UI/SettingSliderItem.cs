using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingSliderItem : MonoBehaviour, ISettingItem
{
    [SerializeField] Image highlight;
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text valueText;

    [SerializeField] float step = 0.05f;

    public bool CanAdjust => true;
    public bool CanSubmit => false;

    public void SetSelected(bool selected)
    {
        highlight.enabled = selected;
    }

    public void Adjust(int dir)
    {
        slider.value = Mathf.Clamp01(slider.value + step * dir);
        valueText.text = Mathf.RoundToInt(slider.value * 100f).ToString();

        // AudioMixer 적용
        // AudioManager.SetBgmVolume(slider.value);
    }

    public void Submit() { }
}

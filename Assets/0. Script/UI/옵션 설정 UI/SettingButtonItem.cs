using UnityEngine;
using UnityEngine.UI;

public class SettingButtonItem : MonoBehaviour, ISettingItem
{
    [SerializeField] Image highlight;
    [SerializeField] Button button;

    public bool CanAdjust => false;
    public bool CanSubmit => true;

    public UIRepeatMode RepeatMode => UIRepeatMode.None;
    public float RepeatInterval => 0f;
    public float AccelStartDelay => 0f;
    public float AccelInitialInterval => 0f;
    public float AccelMinInterval => 0f;
    public float AccelFactor => 0f;

    public void SetSelected(bool selected)
    {
        highlight.enabled = selected;
    }

    public void Adjust(int dir) { }

    public void Submit()
    {
        button.onClick.Invoke();
    }
}

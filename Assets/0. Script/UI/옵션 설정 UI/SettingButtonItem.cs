using UnityEngine;
using UnityEngine.UI;

public class SettingButtonItem : MonoBehaviour, ISettingItem
{
    [SerializeField] Image highlight;
    [SerializeField] Button button;

    public bool CanAdjust => false;
    public bool CanSubmit => true;

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

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingCycleItem : MonoBehaviour, ISettingItem
{
    [SerializeField] Image highlight;
    [SerializeField] TMP_Text valueText;

    [System.Serializable]
    public struct ResolutionEntry
    {
        public int width;
        public int height;
    }

    [SerializeField] List<ResolutionEntry> resolutions;
    int index;

    public bool CanAdjust => true;
    public bool CanSubmit => false;

    public void SetSelected(bool selected)
    {
        highlight.enabled = selected;
    }

    public void Adjust(int dir)
    {
        index = (index + dir + resolutions.Count) % resolutions.Count;
        var r = resolutions[index];
        valueText.text = $"{r.width} x {r.height}";
    }

    public void Submit() { }
}

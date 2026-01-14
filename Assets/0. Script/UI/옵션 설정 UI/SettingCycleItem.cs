using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingCycleItem : MonoBehaviour, ISettingItem
{
    [SerializeField] Image highlight;
    [SerializeField] TMPro.TMP_Text valueText;
    [Header("Optional Buttons (+ / -)")]
    [SerializeField] HoldRepeatButton minusHold;
    [SerializeField] HoldRepeatButton plusHold;

    struct ResolutionEntry
    {
        public int width;
        public int height;
    }

    List<ResolutionEntry> resolutions = new();
    int index;

    public bool CanAdjust => true;
    public bool CanSubmit => false;

    // ---- Repeat 정책(고정) ----
    public UIRepeatMode RepeatMode => UIRepeatMode.FixedInterval;
    public float RepeatInterval => 0.2f;

    // unused (가속용)
    public float AccelStartDelay => 0.2f;
    public float AccelInitialInterval => 0.2f;
    public float AccelMinInterval => 0.2f;
    public float AccelFactor => 1f;

    void Awake()
    {
        BuildResolutions();
        SyncWithCurrentResolution();
        minusHold.OnFire = () => Adjust(-1);
        plusHold.OnFire = () => Adjust(+1);
        ApplyHoldParams(minusHold);
        ApplyHoldParams(plusHold);
    }

    void ApplyHoldParams(HoldRepeatButton hb)
    {
        hb.startDelay = AccelStartDelay;
        hb.initialInterval = AccelInitialInterval;
        hb.minInterval = AccelMinInterval;
        hb.accelFactor = AccelFactor;
    }

    public void SetSelected(bool selected)
    {
        highlight.enabled = selected;
    }

    public void Adjust(int dir)
    {
        if (resolutions.Count == 0) return;
        index = (index + dir + resolutions.Count) % resolutions.Count;
        UpdateText();
    }

    public void Submit() { }

    void BuildResolutions()
    {
        HashSet<(int, int)> unique = new();
        foreach (var r in Screen.resolutions)
        {
            if (unique.Add((r.width, r.height)))
                resolutions.Add(new ResolutionEntry { width = r.width, height = r.height });
        }

        resolutions.Sort((a, b) =>
        {
            int cmp = a.width.CompareTo(b.width);
            return cmp != 0 ? cmp : a.height.CompareTo(b.height);
        });
    }

    void SyncWithCurrentResolution()
    {
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                index = i;
                UpdateText();
                return;
            }
        }

        index = 0;
        UpdateText();
    }

    void UpdateText()
    {
        var r = resolutions[index];
        valueText.text = $"{r.width} x {r.height}";
    }
}

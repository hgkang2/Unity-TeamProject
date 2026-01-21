using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingApplyItem : MonoBehaviour, ISettingItem
{
    [Header("UI")]
    [SerializeField] Image focusHighlight;     // 포커스(선택) 하이라이트
    [SerializeField] Image dirtyIndicator;     // 변경됨 표시(hover 무관)
    [SerializeField] float pulseScale = 1.06f;
    [SerializeField] float pulseDuration = 0.6f;

    Tween pulseTween;

    public bool CanAdjust => false;
    public bool CanSubmit => SettingsManager.CanApply;

    public UIRepeatMode RepeatMode => UIRepeatMode.None;
    public float RepeatInterval => 0f;
    public float AccelStartDelay => 0f;
    public float AccelInitialInterval => 0f;
    public float AccelMinInterval => 0f;
    public float AccelFactor => 1f;

    void OnEnable()
    {
        SettingsManager.DirtyChanged += OnDirtyChanged;
        OnDirtyChanged(SettingsManager.IsDirty);
    }

    void OnDisable()
    {
        SettingsManager.DirtyChanged -= OnDirtyChanged;
        StopPulse();
    }

    public void SetSelected(bool selected)
    {
        if (focusHighlight != null)
            focusHighlight.enabled = selected;
    }

    public void Adjust(int dir) { }

    public void Submit()
    {
        if (!SettingsManager.CanApply) return;
        SettingsManager.CommitAndSave();
    }

    void OnDirtyChanged(bool dirty)
    {
        if (dirtyIndicator != null)
            dirtyIndicator.enabled = dirty;

        if (dirty) StartPulse();
        else StopPulse();
    }

    void StartPulse()
    {
        StopPulse();
        pulseTween = transform
            .DOScale(pulseScale, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    void StopPulse()
    {
        pulseTween?.Kill();
        pulseTween = null;
        transform.localScale = Vector3.one;
    }
}

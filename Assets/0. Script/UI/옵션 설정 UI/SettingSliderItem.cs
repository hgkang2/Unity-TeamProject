using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingSliderItem : MonoBehaviour, ISettingItem
{
    [Header("볼륨에 따라 변경할 스피커 이미지 4개 (0,1,2,3 단계)")]
    [SerializeField] Sprite[] speakerSprites; // 4개
    [SerializeField] Image speakerImage;

    [Header("UI")]
    [SerializeField] Image highlight;
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text valueText;

    [Header("Optional Buttons (+ / -)")]
    [SerializeField] Button minusButton;
    [SerializeField] Button plusButton;

    [Header("Step")]
    [SerializeField] int stepPercent = 5; // 5단위

    int curPercent;

    public bool CanAdjust => true;
    public bool CanSubmit => false;

    public UIRepeatMode RepeatMode => UIRepeatMode.Accelerate;
    public float RepeatInterval => 0f;        // unused
    public float AccelStartDelay => 0.15f;
    public float AccelInitialInterval => 0.15f;
    public float AccelMinInterval => 0.05f;
    public float AccelFactor => 0.8f;

    void Awake()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        minusButton.onClick.AddListener(() => Step(-1));
        plusButton.onClick.AddListener(() => Step(+1));

        // 초기값 동기화 (현재 슬라이더 값 기준)
        SetPercent(PercentFromSlider(slider != null ? slider.value : 0f));
    }
    void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    public void SetSelected(bool selected)
    {
        highlight.enabled = selected;
    }

    public void Adjust(int dir)
    {
        Step(dir);
    }

    void Step(int dir)
    {
        int next = curPercent + dir * stepPercent;
        SetPercent(next);
    }

    // 슬라이더를 마우스로 조절할 때 호출됨
    void OnSliderValueChanged(float _)
    {
        // 마우스 드래그 값 -> 퍼센트로 변환 후 스냅
        int next = PercentFromSlider(slider.value);
        SetPercent(next);
    }

    // 퍼센트 적용의 단일 진입점
    void SetPercent(int percent)
    {
        percent = QuantizePercent(percent);
        percent = Mathf.Clamp(percent, 0, 100);

        // 이미 같은 값이면 불필요한 업데이트/콜백 방지
        if (curPercent == percent)
            return;

        curPercent = percent;

        // 슬라이더 값 동기화 (이때 onValueChanged 재발화 방지)
        if (slider != null)
            slider.SetValueWithoutNotify(curPercent / 100f);

        // 텍스트 갱신
        if (valueText != null)
            valueText.text = curPercent.ToString();

        // 스피커 스프라이트 갱신 (0, 33, 66, 100 이하 조건)
        ApplySpeakerSprite(curPercent);

        // 실제 볼륨 변경
        SoundManager.Instance?.SetBgmVolume(curPercent / 100f);
    }

    int QuantizePercent(int percent)
    {
        // 5단위 스냅: 0,5,10...
        if (stepPercent <= 1) return percent;

        // 가장 가까운 step으로 반올림
        int snapped = Mathf.RoundToInt(percent / (float)stepPercent) * stepPercent;
        return snapped;
    }

    int PercentFromSlider(float v01)
    {
        // 0~1 -> 0~100
        int raw = Mathf.RoundToInt(v01 * 100f);
        return QuantizePercent(raw);
    }

    void ApplySpeakerSprite(int percent)
    {
        int idx;
        if (percent <= 0) idx = 0;
        else if (percent <= 33) idx = 1;
        else if (percent <= 66) idx = 2;
        else idx = 3;

        speakerImage.sprite = speakerSprites[idx];
    }

    public void Submit() { }
}

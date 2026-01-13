public enum UIRepeatMode
{
    None,
    FixedInterval,
    Accelerate
}

public interface ISettingItem
{
    void SetSelected(bool selected);

    bool CanAdjust { get; }
    void Adjust(int dir);   // -1 / +1

    bool CanSubmit { get; }
    void Submit();

    // ---- Repeat 정책 ----
    UIRepeatMode RepeatMode { get; }

    // FixedInterval 용
    float RepeatInterval { get; }     // 예: 0.25f

    // Accelerate 용
    float AccelStartDelay { get; }    // 처음 누른 뒤 다음 2번째 입력까지 지연
    float AccelInitialInterval { get; } // 가속 시작 후 첫 interval
    float AccelMinInterval { get; }     // 최소 interval
    float AccelFactor { get; }          // interval 감소 배율(0~1), 예: 0.85
}

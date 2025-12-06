using UnityEngine;
using DG.Tweening;

public static class TimeManager
{
    public static bool IsPaused { get; private set; }
    public static float CurrentScale { get; private set; } = 1f;

    static Tween activeTween;

    public static event System.Action<float> OnTimeScaleChanged;

    /// <summary>
    /// 시간 완전 정지
    /// </summary>
    public static void Pause()
    {
        KillTween();
        SetTimeScale(0f);
        IsPaused = true;
    }

    /// <summary>
    /// 일시정지 해제 (정상 속도로 복귀)
    /// </summary>
    public static void Resume()
    {
        KillTween();
        SetTimeScale(1f);
        IsPaused = false;
    }

    /// <summary>
    /// 즉시 특정 배속으로 설정
    /// </summary>
    public static void SetTimeScale(float scale)
    {
        Time.timeScale = Mathf.Clamp(scale, 0f, 10f);
        CurrentScale = Time.timeScale;
        OnTimeScaleChanged?.Invoke(CurrentScale);
    }

    /// <summary>
    /// 느려졌다가 복귀하는 단일 효과 (ex. DeathSlow)
    /// </summary>
    public static void SlowForMoment(float slowScale, float slowDuration, float recoverDuration, Ease easeOut = Ease.OutQuad)
    {
        KillTween();

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); // TimeScale과 무관하게 진행

        // 느려지기
        seq.Append(DOTween.To(() => Time.timeScale, x => SetTimeScale(x), slowScale, 0.1f).SetEase(Ease.OutQuad));

        // 유지
        seq.AppendInterval(slowDuration);

        // 복귀
        seq.Append(DOTween.To(() => Time.timeScale, x => SetTimeScale(x), 1f, recoverDuration).SetEase(easeOut));

        activeTween = seq;
    }

    /// <summary>
    /// 현재 트윈 중단
    /// </summary>
    static void KillTween()
    {
        if (activeTween != null && activeTween.IsActive())
        {
            activeTween.Kill();
            activeTween = null;
        }
    }
}

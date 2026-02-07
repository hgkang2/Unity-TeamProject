using UnityEngine;
using DG.Tweening;
using System.Diagnostics;

public static class TimeManager
{
    public static bool IsPaused { get; private set; }
    public static float CurrentScale { get; private set; } = 1f;

    static Tween activeTween;

    public static event System.Action<float> OnTimeScaleChanged;

    const int TargetFPS = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        Application.targetFrameRate = TargetFPS;
        QualitySettings.vSyncCount = 0; // 수직 동기화(VSync) 비활성화 (프레임 강제 제어를 위함)
    }


    static bool timeScaleLocked;

    public static void Pause()
    {
        timeScaleLocked = true;
        KillTween();
        IsPaused = true;
        SetTimeScale(0f);
    }

    public static void Resume()
    {
        timeScaleLocked = false;
        KillTween();
        IsPaused = false;
        SetTimeScale(1f);
    }

    /// <summary>
    /// 즉시 특정 배속으로 설정
    /// </summary>
    public static void SetTimeScale(float scale)
    {
        //pause 중에는 다른 세팅 금지
        if (timeScaleLocked && scale != 0f) return;

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

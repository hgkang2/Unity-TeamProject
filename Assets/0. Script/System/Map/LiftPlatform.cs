using UnityEngine;
using DG.Tweening;

public class LiftPlatform : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] Transform platform;
    [SerializeField] Transform startPos;
    [SerializeField] Transform endPos;

    [Header("Timing")]
    [SerializeField] float duration = 2f;
    [SerializeField] float delay = 1f;

    [Header("Options")]
    [SerializeField] bool autoLoop = true; // 자동 왕복 여부

    bool isMoving = false;

    private void Start()
    {
        if (autoLoop)
            StartLoop();
    }

    public void StartLoop()
    {
        if (isMoving) return;

        isMoving = true;
        LoopToEnd();
    }

    // -------------------------
    // 내부 루프 처리
    // -------------------------

    void LoopToEnd()
    {
        platform.DOLocalMove(endPos.localPosition, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(delay, LoopToStart);
            });
    }

    void LoopToStart()
    {
        platform.DOLocalMove(startPos.localPosition, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(delay, LoopToEnd);
            });
    }
    
    // -------------------------
    // 외부에서 호출할 함수들
    // -------------------------

    public void MoveToStart()
    {
        StopAllTweens();
        platform.DOLocalMove(startPos.localPosition, duration);
    }

    public void MoveToEnd()
    {
        StopAllTweens();
        platform.DOLocalMove(endPos.localPosition, duration);
    }

    // 현재 실행 중인 tween 모두 정리
    void StopAllTweens()
    {
        isMoving = false;
        DOTween.Kill(transform);
    }
}

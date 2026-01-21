using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoldRepeatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Action OnFire;

    [Header("Hold Repeat Params")]
    public float startDelay = 0.15f;
    public float initialInterval = 0.15f;
    public float minInterval = 0.05f;
    public float accelFactor = 0.8f;

    bool holding;
    float nextTime;
    float interval;
    bool repeatingStarted;

    public void OnPointerDown(PointerEventData eventData)
    {
        holding = true;

        // 누르는 순간 1회
        OnFire?.Invoke();

        repeatingStarted = false;
        interval = initialInterval;
        nextTime = Time.unscaledTime + startDelay;
    }

    public void OnPointerUp(PointerEventData eventData) => Stop();
    public void OnPointerExit(PointerEventData eventData) => Stop();

    void Update()
    {
        if (!holding) return;
        if (Time.unscaledTime < nextTime) return;

        OnFire?.Invoke();

        if (!repeatingStarted)
            repeatingStarted = true;
        else
            interval = Mathf.Max(minInterval, interval * accelFactor);

        nextTime = Time.unscaledTime + interval;
    }

    void Stop()
    {
        holding = false;
        repeatingStarted = false;
    }
}

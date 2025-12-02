using System.Collections;
using UnityEngine;

public class SpriteFlash : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] renderers;

    [Header("Hit Flash")]
    [SerializeField] Color hitColor = Color.white;
    [SerializeField] float hitFlashDuration = 0.05f;

    [Header("Invincible Blink")]
    [SerializeField] Color blinkColor = Color.white;
    [SerializeField] float blinkInterval = 0.1f;

    Color[] originalColors;
    Coroutine blinkRoutine;

    void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }
    }

    // ----------------------------
    // 피격 순간 한번 번쩍
    // ----------------------------
    public void PlayHitFlash()
    {
        StartCoroutine(HitFlashRoutine());
    }

    IEnumerator HitFlashRoutine()
    {
        // 히트 색으로 변경
        SetAllColors(hitColor);
        yield return new WaitForSeconds(hitFlashDuration);
        // 원래 색으로 되돌리기
        RestoreOriginalColors();
    }

    // ----------------------------
    // 무적 깜빡임 시작/종료
    // ----------------------------
    public void StartInvincibleBlink()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    public void StopInvincibleBlink()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = null;
        RestoreOriginalColors();
    }

    IEnumerator BlinkRoutine()
    {
        bool useBlinkColor = false;

        while (true)
        {
            useBlinkColor = !useBlinkColor;

            if (useBlinkColor)
                SetAllColors(blinkColor);
            else
                RestoreOriginalColors();

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void SetAllColors(Color c)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = c;
        }
    }

    void RestoreOriginalColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = originalColors[i];
        }
    }
}

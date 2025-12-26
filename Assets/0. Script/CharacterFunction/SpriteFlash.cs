using System.Collections;
using UnityEngine;

public class SpriteFlash : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] renderers;

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
    // 무적 깜빡임 시작/종료
    // ----------------------------
    public void StartInvincibleBlink()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    public void StartInvincibleBlink(float duration)
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkForDurationRoutine(duration));
    }

    public void StopInvincibleBlink()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = null;
        RestoreOriginalColors();
    }

    IEnumerator BlinkForDurationRoutine(float duration)
    {
        Coroutine blink = StartCoroutine(BlinkRoutine());

        yield return new WaitForSeconds(duration);

        StopCoroutine(blink);
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

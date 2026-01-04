using System.Collections;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D[] childColliders;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        childColliders = GetComponentsInChildren<Collider2D>();
    }

    
    public IEnumerator StartCycle(float initialDelay, float stayTime)
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
    {
        // 1. 발판 유지 (3초)
        SetAlpha(1f);
        SetCollidersActive(true);
        yield return new WaitForSeconds(stayTime); 

        // 2. 사라지는 중 (2초 연출)
        StartCoroutine(Fade(1f, 0f, 2f)); 
        yield return new WaitForSeconds(0.8f); 
        SetCollidersActive(false); 
        yield return new WaitForSeconds(1.2f); 

        // 3. 투명(잔상) 유지 (2초)
        yield return new WaitForSeconds(2f);

        // 4. 나타나는 중 (0.2초)
        yield return StartCoroutine(Fade(0f, 1f, 0.2f));
    }
    }

    // ... (Fade, SetCollidersActive, SetAlpha 함수는 기존과 동일) ...
    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = spriteRenderer.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            spriteRenderer.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }
        SetAlpha(endAlpha);
    }

    void SetCollidersActive(bool isActive)
    {
        foreach (var col in childColliders) { if (col != null) col.enabled = isActive; }
    }

    void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
    }
}
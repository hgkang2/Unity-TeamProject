using System.Collections;
using UnityEngine;

public class BossLunaExpiation : MonoBehaviour
{
    Rigidbody2D[] chainRb;

    SpriteRenderer spriteRenderer;

    Vector2 EexpiationTargetPos;
    GameObject owner;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        chainRb = GetComponentsInChildren<Rigidbody2D>();
    }

    public void InitializeExpiation(Vector2 tartgetPos, float duration, GameObject owner)
    {
        EexpiationTargetPos = tartgetPos;
        this.owner = owner;

        StartCoroutine(FadeInRoutine(duration));
    }

    IEnumerator FadeInRoutine(float duration)
    {
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            spriteRenderer.color = c;
            yield return null;
        }

        c.a = 1f;
        spriteRenderer.color = c;

        StartCoroutine(ChainAttack());
    }

    IEnumerator ChainAttack()
    {
        yield return new WaitForSeconds(0.3f);

        foreach (var rb in chainRb)
        {
            rb.AddForce(rb.transform.up * 20f, ForceMode2D.Impulse);
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(0.2f);

        foreach (var rb in chainRb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Destroy(this.gameObject, 1f);
    }
}

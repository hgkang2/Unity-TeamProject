using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class KeepDistance : MonoBehaviour
{
    [SerializeField] Transform Target;

    [SerializeField] float MinDistance = 4f;

    [SerializeField] float prePause = 1f; // 접근 후 대기 시간
    [SerializeField] float retreatDuration = 0.5f; // 후퇴 시간
    [SerializeField] float postPause = 2f; // 후퇴후 대기 (재발동 방지)

    [SerializeField] float retreatSpeed;

    Rigidbody2D rb;
    Coroutine coroutine;
    public bool isRetreating { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();    
    }

    public void SetTarget(Transform t) => Target = t;

    public void TryRetreat()
    {
        if (isRetreating) return;
        if (rb == null || Target == null) return;

        float distance = Vector2.Distance(transform.position, Target.position);
        if (distance > MinDistance) return;

        coroutine = StartCoroutine(RetreatRoutine());
    }

    IEnumerator RetreatRoutine()
    {
        isRetreating = true;

        rb.linearVelocity = Vector2.zero;
        if (prePause > 0f) yield return new WaitForSeconds(prePause);

        Vector2 away = ((Vector2)transform.position - (Vector2)Target.position).normalized;
        float t = 0f;
        while(t < retreatDuration)
        {
            rb.linearVelocity = away * retreatSpeed;
            t += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        if (postPause > 0f) yield return new WaitForSeconds(postPause);

        isRetreating = false;
        coroutine = null;
    }

    public void StopRetreat()
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = null;
        isRetreating = false;
    }
}

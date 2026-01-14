using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Chick : MonoBehaviour, IInteractable
{
    [Header("Idle Cycle (Wait <-> Move)")]
    [SerializeField] float waitMin = 1f;
    [SerializeField] float waitMax = 4f;
    [SerializeField] float moveMin = 1f;
    [SerializeField] float moveMax = 4f;

    [Header("Move")]
    [SerializeField] float moveSpeed = 1.2f;

    [Header("Peck")]
    [Range(0f, 1f)]
    [SerializeField] float peckChanceOnTransition = 0.5f;
    [SerializeField] string peckStateTag = "Peck"; // Animator의 쪼기 State에 Tag를 "Peck"로 설정해줘

    [Header("Animator Triggers")]
    [SerializeField] string trigIdle = "Idle";
    [SerializeField] string trigMove = "Move";
    [SerializeField] string trigPeck = "Peck";
    [SerializeField] string trigFly = "Fly";

    [Header("Interact Fly")]
    [SerializeField] float flyDuration = 1f;
    [SerializeField] float flyForwardDistance = 1.2f;
    [SerializeField] float flyUpDistance = 1.0f;

    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;

    Coroutine idleRoutine;
    bool isInteracting;

    Tween highlightTween;
    Collider2D[] allColliders;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        allColliders = GetComponentsInChildren<Collider2D>(true);
    }

    void OnEnable()
    {
        idleRoutine = StartCoroutine(IdleLoop());
    }

    void OnDisable()
    {
        if (idleRoutine != null)
            StopCoroutine(idleRoutine);

        highlightTween?.Kill();
        highlightTween = null;
    }

    void Update()
    {
        if (isInteracting)
            return;

        float vx = rb.linearVelocityX;
        if (Mathf.Abs(vx) < 0.01f)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -Mathf.Sign(vx);
        transform.localScale = scale;
    }

    IEnumerator IdleLoop()
    {
        // 시작은 대기(Idle)
        SetTrigger(trigIdle);

        while (!isInteracting)
        {
            // 1) Wait
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(RandomRange(waitMin, waitMax));
            if (isInteracting) yield break;

            // Wait -> Move 전환 직전 쪼기 체크
            yield return TryPeckOnce();
            if (isInteracting) yield break;

            // 2) Move
            SetTrigger(trigMove);

            float dir = Random.value < 0.5f ? -1f : 1f;
            float moveTime = RandomRange(moveMin, moveMax);

            float t = 0f;
            while (t < moveTime && !isInteracting)
            {
                rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocityY);
                t += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            if (isInteracting) yield break;

            // Move -> Wait 전환 직전 쪼기 체크
            yield return TryPeckOnce();
            if (isInteracting) yield break;

            SetTrigger(trigIdle);
        }
    }

    IEnumerator TryPeckOnce()
    {
        if (Random.value > peckChanceOnTransition)
            yield break;

        // 쪼기는 제자리 고정
        rb.linearVelocity = Vector2.zero;

        SetTrigger(trigPeck);

        // "Has Exit Time"로 자연스럽게 끝나게 두되,
        // 로직은 쪼기 State(태그) 재생이 끝날 때까지 기다렸다가 다음으로 넘어감.
        // (쪼기 State에 Tag = peckStateTag 를 꼭 달아줘)
        yield return WaitForTaggedStateToFinish(peckStateTag);
    }

    IEnumerator WaitForTaggedStateToFinish(string tag)
    {
        // 트리거 직후 1프레임은 아직 이전 state일 수 있음
        yield return null;

        // 1) 쪼기 상태로 "진입"할 때까지 잠깐 대기 (최대 1초 가드)
        float enterGuard = 0f;
        while (!isInteracting && enterGuard < 1f)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            if (st.IsTag(tag))
                break;

            enterGuard += Time.deltaTime;
            yield return null;
        }

        if (isInteracting) yield break;

        // 2) 쪼기 상태가 "끝날 때"까지 대기
        while (!isInteracting)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);

            // 쪼기 태그 상태가 아니면(= 이미 빠져나왔으면) 종료
            if (!st.IsTag(tag))
                break;

            // 같은 state 안에서 normalizedTime >= 1 이면 사실상 끝 (Exit Time로 전환되겠지만 1프레임이라도 더 기다림)
            if (st.normalizedTime >= 1f && !animator.IsInTransition(0))
                break;

            yield return null;
        }
    }

    float RandomRange(float a, float b)
    {
        if (b < a) (a, b) = (b, a);
        return Random.Range(a, b);
    }

    void SetTrigger(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        animator.ResetTrigger(trigIdle);
        animator.ResetTrigger(trigMove);
        animator.ResetTrigger(trigPeck);
        animator.ResetTrigger(trigFly);

        animator.SetTrigger(name);
    }

    // ===== IInteractable =====

    public bool CanInteract()
    {
        return !isInteracting;
    }

    public bool IsAvailable()
    {
        return !isInteracting;
    }

    public void OnFocus()
    {
        HighlightOn();
    }

    public void OnUnfocus()
    {
        HighlightOff();
    }

    public void Exit()
    {
    }

    public void Interact(Player player, Interactor interactor)
    {
        if (isInteracting)
            return;

        isInteracting = true;

        // 진행 중 행동 중지
        if (idleRoutine != null)
        {
            StopCoroutine(idleRoutine);
            idleRoutine = null;
        }

        // 하이라이트는 즉시 원복
        highlightTween?.Kill();
        highlightTween = null;
        spriteRenderer.color = Color.white;

        // 콜라이더 전부 끄기 (본체 + 자식 interactable 포함)
        for (int i = 0; i < allColliders.Length; i++)
            allColliders[i].enabled = false;

        // 물리 완전 차단
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        // 날아가기 애니 트리거
        SetTrigger(trigFly);

        // "앞+위 대각선" (현재 바라보는 방향 기준)
        float facing = -Mathf.Sign(transform.localScale.x); // 스케일 반전 규칙이 음수면 오른쪽 바라보는 식이라면 이 값이 더 자연스럽게 나올 수 있음
        if (Mathf.Abs(facing) < 0.01f) facing = 1f;

        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(facing * flyForwardDistance, flyUpDistance, 0f);

        // 트윈으로 이동 후 파괴
        transform.DOMove(end, flyDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    // ===== Highlight =====

    public void HighlightOn()
    {
        highlightTween?.Kill();
        highlightTween = spriteRenderer.DOColor(new Color(1.5f, 1.5f, 1.5f, 1f), 0.15f);
    }

    public void HighlightOff()
    {
        highlightTween?.Kill();
        highlightTween = spriteRenderer.DOColor(Color.white, 0.15f);
    }
}

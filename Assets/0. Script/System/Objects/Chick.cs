using System.Collections;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class Chick : MonoBehaviour, IInteractable
{
    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    
    bool isPlaying = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        float vx = rb.linearVelocityX;
        
        animator.SetFloat("AbsMoveX", Mathf.Abs(rb.linearVelocityX));
        if (Mathf.Abs(vx) < 0.01f)
        {
            return; // 거의 안 움직일 땐 방향 유지
        }
            

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -Mathf.Sign(vx);
        transform.localScale = scale;
    }

    public bool CanInteract()
    {
        return !isPlaying;
    }

    public void Exit()
    {

    }

    public void Interact(Player player, Interactor interactor)
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Interact");
        isPlaying = true;
        HighlightOff();
        StartCoroutine(Initialize(interactor));
    }
    IEnumerator Initialize(Interactor interactor)
    {
        yield return new WaitForSeconds(2);
        interactor.InteractExit();
        isPlaying = false;
    }

    public bool IsAvailable()
    {
        return !isPlaying;
    }

    public void OnFocus()
    {
        HighlightOn();
        NudgeRandom();
    }

    public void OnUnfocus()
    {
        HighlightOff();
    }

    Tween highlightTween;
    public void HighlightOn()
    {
        highlightTween?.Kill();
        highlightTween = spriteRenderer.DOColor(
            new Color(1.5f, 1.5f, 1.5f, 1f),
            0.15f
        );
    }

    public void HighlightOff()
    {
        highlightTween?.Kill();
        highlightTween = spriteRenderer.DOColor(Color.white, 0.15f);
    }

    [Header("Focus Nudge")]
    [SerializeField] float nudgeForce = 3f;
    [SerializeField] float maxXVelocity = 5f;

    void NudgeRandom()
    {
        // 좌(-1) / 우(+1) 랜덤
        float dir = UnityEngine.Random.value < 0.5f ? -1f : 1f;

        // 기존 X 속도 클램프 (과속 방지)
        Vector2 v = rb.linearVelocity;
        v.x = Mathf.Clamp(v.x, -maxXVelocity, maxXVelocity);
        rb.linearVelocity = v;

        // 순간 힘으로 살짝 이동
        rb.AddForce(new Vector2(dir * nudgeForce, 0f), ForceMode2D.Impulse);
    }
}

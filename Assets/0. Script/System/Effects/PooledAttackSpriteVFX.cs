using UnityEngine;

public class PooledAttackSpriteVFX : MonoBehaviour
{
    Animator animator;
    SpriteRenderer spriteRenderer;

    GameObjectPool myPool;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetPool(GameObjectPool pool)
    {
        myPool = pool;
    }

    void OnEnable()
    {
        // 풀에서 꺼낼 때마다 첫 프레임부터 확실히 재생되게 초기화
        spriteRenderer.enabled = true;

        animator.Rebind();
        animator.Update(0f);
    }

    // AnimationEvent (클립 마지막 프레임에 등록)
    public void OnVFXEnd()
    {
        spriteRenderer.enabled = false;

        if (myPool != null)
        {
            myPool.Return(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }
}

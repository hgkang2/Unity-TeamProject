using System.Collections;
using UnityEngine;

public class Stalactite : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFalling = false;

    [Header("설정")]
    [SerializeField] float damage = 10f;
    [SerializeField] float stunDuration = 1.0f; // 1초 경직
    [SerializeField] float fallGravity = 3.0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; 
        StartCoroutine(FallAfterDelay(2.0f));
    }

    IEnumerator FallAfterDelay(float delay)
    {
        isFalling = true;
        
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        while (elapsed < delay)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.05f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
        rb.gravityScale = fallGravity; 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 플레이어와 충돌 시
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                // Player.cs에 있는 TakeDamage를 호출
                // DamageType.Normal 내부 로직에 의해 ApplyKnockback이 자동 실행됩니다.
                player.TakeDamage(damage, DamageType.Normal, transform.position);

                // 1초 경직을 위해 Player.cs의 StartHitStun을 호출 (리플렉션 방식)
                player.SendMessage("StartHitStun", stunDuration, SendMessageOptions.DontRequireReceiver);
            }
            
            Destroy(gameObject); // 충돌 후 파괴
        }
        // 2. 바닥(Trap이나 Ground)에 부딪혔을 때
        else if (collision.gameObject.CompareTag("Trap") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
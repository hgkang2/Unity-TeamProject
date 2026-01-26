using System.Collections;
using UnityEngine;

public class Stalactite : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFalling = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // 생성 직후에는 공중에 고정
        StartCoroutine(FallAfterDelay(2.0f));
    }

    IEnumerator FallAfterDelay(float delay)
    {
        isFalling = true;
        
        // 2초간 떨리는 연출
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        while (elapsed < delay)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.05f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
        rb.gravityScale = 3.0f; // 중력 적용하여 낙하
    }

    // Is Trigger가 꺼져 있을 때 작동하는 충돌 함수
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 대상의 태그가 Trap라면 삭제
        if (collision.gameObject.CompareTag("Trap"))
        {
            Destroy(gameObject);
        }
        
        // 플레이어와 부딪혔을 때의 로직도 여기에 추가 가능합니다.
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어가 종유석에 맞았습니다!");
            Destroy(gameObject); // 필요시 파괴
        }
    }
}
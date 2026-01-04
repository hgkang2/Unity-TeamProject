using UnityEngine;

public class Checkpoint : MonoBehaviour
{
  
    [SerializeField] private Color inactiveColor = Color.red;
    private SpriteRenderer sr;
    private bool isActivated = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = inactiveColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어인지 확인
        if (collision.CompareTag("Player") && !isActivated)
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // 1. 플레이어의 안전 위치를 이 체크포인트의 위치로 갱신
                player.UpdateSafePosition(transform.position);
                
            }
        }
    }
}
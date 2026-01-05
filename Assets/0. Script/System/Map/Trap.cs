using Unity.VisualScripting;
using UnityEngine;

public class Trap : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();
        if (player != null)
        {
            player.TakeTrapDamage(); // 함정 데미지 호출
        }
    }
}
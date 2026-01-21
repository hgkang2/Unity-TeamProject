using UnityEngine;


public class BeadObject : MonoBehaviour
{
    // 각각 공격력 상승 효과, 플레이어 힐, 플레이어 이속 증가
    [SerializeField] SoulData soulData;

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어한테 닿으면 없어지면서 효과 적용.
        if(collision.gameObject.CompareTag("Player"))
        {
            SoulManager.Instance.EnrollSoul(soulData);
            Destroy(gameObject);
        }
    }
}

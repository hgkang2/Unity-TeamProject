using UnityEngine;

public class Lava : MonoBehaviour
{
    public float damage = 10f; // 데미지 양
    public float tick = 0.5f;  // 데미지 주기
    private float timer;

    // 들어갔을 때 즉시 데미지
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (target == null) return;
        
        // Player의 TakeDamage(데미지, 타입, 위치) 형식에 맞춤
        // 용암의 위치(transform.position)를 넘겨주면 넉백 방향이 자연스러워집니다.
        target.TakeDamage(damage, DamageType.Area, transform.position);
    }

    // 머물러 있을 때 지속 데미지
    void OnTriggerStay2D(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (target == null) return;

        timer += Time.deltaTime;

        if (timer >= tick)
        {
            timer = 0f;
            target.TakeDamage(damage, DamageType.Area, transform.position);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        timer = 0f;
    }
}
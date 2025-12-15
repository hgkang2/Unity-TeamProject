using Unity.VisualScripting;
using UnityEngine;

public class Lava : MonoBehaviour
{
    public float damage = 1;
    public float tick = 0.2f;
    public float timer;

    public float Damage => damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (target == null) return;
        target.TakeDamage(damage, DamageType.Area);
    }
    void OnTriggerStay2D(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (target == null) return;

        timer += Time.deltaTime;

        if (timer >= tick)
        {
            timer = 0f;
            target.TakeDamage(damage, DamageType.Area);
        }
    }
    void OnTriggerExit2D(Collider2D other) {
        timer = 0f;
    }
}

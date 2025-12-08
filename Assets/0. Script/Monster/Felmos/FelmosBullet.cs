using UnityEngine;

public class FelmosBullet : MonoBehaviour
{
    Collider2D cd;
    Rigidbody2D rb;
    Player player;

    MonsterBase monster;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<Collider2D>();
    }

    public void Initialize(Vector2 direction)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * 20f, ForceMode2D.Impulse);

        Destroy(this.gameObject, 5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            float damage = monster.monsterData.Skill_Damage;
            player.TakeDamage(damage);

            Destroy(this.gameObject, 0.5f);
        }

        if(collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground"))
        {
            Destroy(this.gameObject);
        }
    }
}

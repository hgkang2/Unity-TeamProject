using UnityEngine;

public class FelmosBullet : MonoBehaviour, IDamageable
{
    Transform PlayerPos;
    Transform Felmos;
    Vector2 dir;

    Collider2D cd;
    Rigidbody2D rb;

    private void Awake()
    {
        PlayerPos = GameObject.Find("Player").GetComponent<Transform>();
        Felmos = GameObject.Find("Felmos").GetComponent<Transform>();
        dir = PlayerPos.position - Felmos.transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.AddForce(dir.normalized * 20f, ForceMode2D.Impulse);
    }

    void Update()
    {
        
    }

    public void TakeDamage(float amount)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        throw new System.NotImplementedException();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground"))
        {
            Destroy(this.gameObject);
        }
    }
}

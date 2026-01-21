using UnityEngine;

public class MonsterFlyingMove : MonoBehaviour
{
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void StopAll()
    {
        rb.linearVelocity = new Vector2(0f, 0f);
    }

    public void Move(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction * speed;
    }
}

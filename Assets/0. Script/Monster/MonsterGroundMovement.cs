using UnityEngine;

public class MonsterGroundMovement : MonoBehaviour
{
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void StopX()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void MoveX(int dirX, float speed)
    {
        rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
    }

    public void KnockbackX(float xVel)
    {
        rb.linearVelocity = new Vector2(xVel, rb.linearVelocity.y);
    }
}

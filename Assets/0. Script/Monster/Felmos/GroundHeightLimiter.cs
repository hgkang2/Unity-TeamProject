using UnityEngine;

public class GroundHeightLimiter : MonoBehaviour
{
    [SerializeField] float MinHeight = 3f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, MinHeight, groundLayer);

        Debug.DrawRay(transform.position, Vector2.down * MinHeight, Color.purple);

        if (hit.collider != null)
        {
            var v = rb.linearVelocity;
            if (v.y < 0f)
                rb.linearVelocity = new Vector2(v.x, 0f);
        }
    }

    public void SetMinHeight(float Value) => MinHeight = Mathf.Max(0f, Value);
}

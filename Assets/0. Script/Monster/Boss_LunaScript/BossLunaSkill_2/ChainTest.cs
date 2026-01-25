using UnityEngine;

public class ChainTest : MonoBehaviour
{
    Rigidbody2D rb;
    float stop;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.AddForce(transform.up * 20f, ForceMode2D.Impulse);    
    }

    // Update is called once per frame
    void Update()
    {
        stop += Time.deltaTime;
        if(stop >= 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}

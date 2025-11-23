using UnityEngine;

public class HPPlayer : HP
{
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D collider2d;

    void Start()
    {   
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        collider2d = GetComponent<Collider2D>();
}

    [System.Obsolete]
    protected override void OnDiedInternal()
    {
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }
        if (rb != null)
        {
            rb.velocity = Vector2.zero; 
            rb.isKinematic = true; 
        }

        if (collider2d != null)
        {
            collider2d.enabled = false;
        }
        Destroy(gameObject, 2f); 
    }
}

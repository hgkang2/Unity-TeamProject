using UnityEngine;

public class HolyGrenadeExplode : MonoBehaviour
{
    Animator animator;

    Collider2D innerExplosionRadius;
    Collider2D centerExplosionRadius;
    Collider2D outerExplosionRadius;

    void Awake()
    {
        //if(gameObject) gameObject.SetActive(false);
        animator = GetComponent<Animator>();
    }

    void OnExplode()
    {
        
    }

    void OnExplodeEnd()
    {
        Destroy(this.gameObject);
    }
}

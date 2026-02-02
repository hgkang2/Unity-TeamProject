using UnityEngine;

public class TriggerRelay : MonoBehaviour
{
    AltarItem owner;
    Collider2D myCollider;

    void Awake()
    {
        owner = GetComponentInParent<AltarItem>();
        myCollider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        owner.HandleTriggerEnter(myCollider, other);
    }
}

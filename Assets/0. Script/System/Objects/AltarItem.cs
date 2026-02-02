using UnityEngine;

public class AltarItem : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField] Collider2D detectTrigger;
    [SerializeField] Collider2D pickupTrigger;

    [Header("Magnet")]
    [SerializeField] float attractSpeed = 8f;

    Transform player;
    bool isAttracting;

    void FixedUpdate()
    {
        if (!isAttracting || player == null)
            return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * attractSpeed * Time.fixedDeltaTime);
    }

    public void HandleTriggerEnter(Collider2D trigger, Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (trigger == detectTrigger)
        {
            player = other.transform;
            isAttracting = true;
        }
        else if (trigger == pickupTrigger)
        {
            GameManager.Instance.AddFlame();
            Destroy(gameObject);
        }
    }
}

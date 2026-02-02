using UnityEngine;

public class BossLunaGenesisLight : MonoBehaviour
{
    [SerializeField] float tickInterval;
    [SerializeField] float damage;
    [SerializeField] DamageType damageType;
    IDamageable playerDmg;
    bool inBeam;
    float nextTickTime;
    Transform playerTr;

    void Awake()
    {
        damageType = GetComponent<DamageType>();
    }

    void Update()
    {
        if(!inBeam) return;

        if(Time.time >= nextTickTime)
        {
            nextTickTime = Time.time + tickInterval;

            Vector2 hitPos = playerTr ? (Vector2)playerTr.position : (Vector2)transform.position;
            playerDmg.TakeDamage(damage, DamageType.Normal, hitPos);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player")) return;

        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            inBeam = true;
            playerDmg = damageable;
            playerTr = collision.transform;

            nextTickTime = Time.time;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player")) return;

        inBeam = false;
        playerDmg = null;
        playerTr = null;
    }
}
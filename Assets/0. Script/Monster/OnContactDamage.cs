using UnityEngine;

public class OnContactDamage : MonoBehaviour
{
    [SerializeField] private MonsterBase monster;
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        if (monster == null) monster = GetComponent<MonsterBase>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag)) return;

        Player player = collision.GetComponent<Player>();
        if (player == null) return;

        player.TakeDamage(monster.monsterStats.colideDamage, DamageType.Normal);
    }
}

using UnityEngine;

public class MonsterDetecter : MonoBehaviour
{
    MonsterBase monsterBase;

    private void Awake()
    {
        monsterBase = GetComponent<MonsterBase>();
    }

    private void FixedUpdate()
    {
        if (TimeManager.IsPaused) return;
        DetectPlayer();
    }

    void DetectPlayer()
    {
        if (monsterBase == null) return;

        float aggroRange = monsterBase.monsterStats.aggroRange;
        LayerMask playermask = monsterBase.PlayerLayermask;

        Collider2D detectCollider = Physics2D.OverlapCircle(transform.position, aggroRange, playermask);

        if(detectCollider != null && detectCollider.CompareTag("Player") && !monsterBase.isUsingSkill)
        {
            monsterBase.PlayerPosition = detectCollider.transform;

            Vector2 playerPos = new Vector2(monsterBase.PlayerPosition.position.x, monsterBase.PlayerPosition.position.y);
            Vector2 myPos = new Vector2(transform.position.x, transform.position.y);

            monsterBase.direction = (playerPos - myPos).normalized;

            monsterBase.DistanceToPlayer = Vector2.Distance(transform.position, monsterBase.PlayerPosition.position);
        }
        else
        {
            monsterBase.PlayerPosition = null;
            monsterBase.DistanceToPlayer = Mathf.Infinity;
        }
    }

    protected virtual Vector2 GetTargetPosition(Vector2 playerWorldPos)
    {
        return new Vector2(playerWorldPos.x, transform.position.y);
    }
}

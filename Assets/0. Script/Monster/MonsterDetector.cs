using UnityEngine;

public class MonsterDetector : MonoBehaviour
{
    public LayerMask playerMask;
    public float aggroRange = 0f;

    [Header("Detector")]
    public Transform player;
    public float distance;
    public Vector2 dirToPlayer;
    public int moveDirx;

    public float dx;
    public float deadZoneX = 0.1f;

    public bool hasPlayer => player != null;

    public void Detector(MonsterBase monsterBase)
    {
        aggroRange = monsterBase.monsterStats.aggroRange;

        if(player ==null)
        {
            var p = FindFirstObjectByType<Player>();
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        Vector2 myPos = transform.position;
        Vector2 playerPos = player.position;

        Vector2 lengthToPlayer = playerPos - myPos;

        dx = lengthToPlayer.x;

        distance = lengthToPlayer.magnitude;

        dirToPlayer = distance > 0.001f ? lengthToPlayer / distance : Vector2.zero;

        if (Mathf.Abs(dx) > deadZoneX)
            moveDirx = dx > 0f ? 1 : -1;
    }

    public bool InAggroRange(MonsterBase monsterBase)
    {
        return hasPlayer && distance <= monsterBase.monsterStats.aggroRange;
    }
}

using UnityEngine;

public struct HitContext
{
    public float amount;                 // 데미지(없으면 0)
    public DamageType type;              // 네 enum 그대로 재사용
    public Vector2 point;                // 맞은 지점
    public Vector2 normal;               // 표면 노말
    public Vector2? attackerWorldPos;    // 공격자 위치(있으면 넉백 방향 등에 사용)
    public GameObject instigator;        // 공격 주체(플레이어/몬스터)
}
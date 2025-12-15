using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStats_", menuName = "Monster/Stats")]
public class MonsterStats : ScriptableObject
{
    [Header("Reward")]
    public int exp;

    [Header("Idle / Patrol")]
    public float idleTime;
    public float moveDirection;
    public float patrolTime;
    public float patrolSpeed;

    [Header("Aggro")]
    public float aggroRange;
    public float aggroSpeed;

    [Header("Attack")]
    public float attackRange;
    public float attackDamage;
    public float attackRate;

    [Header("Damage")]
    public float skillDamage;
    public float colideDamage;

    [Header("Skill")]
    public float skillDelay;
    public float skillActiveRange;
    public float skillCoolTime;

    [Header("Hit Reaction")]
    public float hitStunTime;
    public float knockbackPower;
}

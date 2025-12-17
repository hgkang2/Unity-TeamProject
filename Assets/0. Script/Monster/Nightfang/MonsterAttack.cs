using UnityEngine;
using System;

public class MonsterAttack : MonoBehaviour
{
    private MonsterBase monster;
    [SerializeField] SkillHitBox hitBox; // �� �ڵ尡 �پ� �ִ� ��ũ��Ʈ

    private void Awake()
    {   
        monster = GetComponent<Nightfang>();
    }

    private void OnEnable()
    {
        hitBox.OnHit += HandleHit;
    }

    private void OnDisable()
    {
        hitBox.OnHit -= HandleHit;
    }

    private void HandleHit(IDamageable target)
    {
        float damage = monster.monsterStats.skillDamage;

        Debug.Log("[MonsterAttack] HandleHit ȣ��, ������:" + damage);
        target.TakeDamage(damage, DamageType.Normal, transform.position);
    }
}

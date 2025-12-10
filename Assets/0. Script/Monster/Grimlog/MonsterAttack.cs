using UnityEngine;
using System;

public class MonsterAttack : MonoBehaviour
{
    private MonsterBase monster;
    [SerializeField] SkillHitBox hitBox; // �� �ڵ尡 �پ� �ִ� ��ũ��Ʈ

    private void Awake()
    {   
        monster = GetComponent<Grimlog>();
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
        float damage = monster.monsterData.Skill_Damage;

        Debug.Log("[MonsterAttack] HandleHit ȣ��, ������:" + damage);
        target.TakeDamage(damage, transform.position);
    }
}

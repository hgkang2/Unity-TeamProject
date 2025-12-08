using UnityEngine;
using System;

public class MonsterAttack : MonoBehaviour
{
    private MonsterBase monster;
    [SerializeField] SkillHitBox hitBox; // 위 코드가 붙어 있는 스크립트

    private void Awake()
    {
        if (hitBox == null)
            hitBox = GetComponent<SkillHitBox>();
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

        Debug.Log("[MonsterAttack] HandleHit 호출, 데미지:" + damage);
        target.TakeDamage(damage);
    }
}

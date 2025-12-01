using System;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    HP hp;
    public HP HP { get { return hp; } }

    Exp exp;
    public Exp Exp { get { return exp; } }

    PlayerStats stats;
    public PlayerStats Stats { get { return stats; } }

    PlayerMove move;

    public GameObject playerSprite;

    public void DisablePlayerSprite()
    {
        if (playerSprite != null)
        {
            playerSprite.SetActive(false);
        }
    }
    public void EnablePlayerSprite()
    {
        playerSprite.SetActive(true);
    }

    void Awake()
    {
        hp = GetComponent<HP>();
        exp = GetComponent<Exp>();
        stats = GetComponent<PlayerStats>();
        move = GetComponent<PlayerMove>();
    }

    void OnEnable()
    {
        hp.OnDied += HandleDie;
    }

    void OnDisable()
    {
        hp.OnDied -= HandleDie;
    }

    void HandleDie()
    {
        move.HandleDieMotion();

        Destroy(gameObject, 2f);
    }

    // IDamageable 기본 버전 (공격자 위치 모를 때)
    public void TakeDamage(float amount)
    {
        if (HP.IsDead) return;

        hp.TakeDamage(amount);
        move.StartKnockbackByFacing();
    }

    // 공격자 위치를 아는 버전
    public void TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        if (HP.IsDead) return;

        hp.TakeDamage(amount);
        move.StartKnockbackFromAttacker(attackerWorldPosition);
    }
}

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

    PlayerMove playerMove;
    PlayerAttack playerAttack;

    SpriteRenderer playerSprite;
    public GameObject playerPartSprite;
    
    SpriteFlash spriteFlash;

    public bool CanControl
    {
        get
        {
            // 필요하면 isDead, 컷신 등 같이 묶어서 처리
            return !isStunned;
        }
    }


    void Awake()
    {
        hp = GetComponent<HP>();
        exp = GetComponent<Exp>();
        stats = GetComponent<PlayerStats>();
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerSprite = GetComponent<SpriteRenderer>();
        spriteFlash = GetComponent<SpriteFlash>();
    }

    void OnEnable()
    {
        hp.OnDied += HandleDie;
    }

    void OnDisable()
    {
        hp.OnDied -= HandleDie;
    }

    void Update()
    {
        UpdateHitStun();
        UpdateInvincible();
    }




    [Header("피격 경직 시간")]
    [SerializeField] float hitStunDuration = 0.15f;
    float stunTimer;
    bool isStunned;
    public bool IsStunned => isStunned;
    void UpdateHitStun()
    {
        if (!isStunned)
            return;

        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            isStunned = false;
            stunTimer = 0f;
        }
    }

    [Header("피격 무적 시간")]
    [SerializeField] float hitInvincibleDuration = 0.5f;

    float invincibleTimer;
    bool isInvincible;
    public bool IsInvincible => isInvincible;

    void UpdateInvincible()
    {
        if (!isInvincible)
            return;

        invincibleTimer -= Time.deltaTime;
        if (invincibleTimer <= 0f)
        {
            isInvincible = false;
            invincibleTimer = 0f;
            spriteFlash.StopInvincibleBlink();
        }
    }

    // IDamageable 기본 버전 (공격자 위치 모를 때)
    public void TakeDamage(float amount)
    {
        if (isInvincible) return; //경직 무적
        if (playerMove.IsDodging) return; //구르기 무적

        hp.TakeDamage(amount);

        // 2. 경직 적용
        float stunDuration = hitStunDuration;
        if (stunDuration > 0f)
        {
            isStunned = true;
            stunTimer = stunDuration;
            InterruptOnHit();
            // TODO : 경직 모션 추가
        }

        // 3. 피격 후 무적
        float invDuration = hitInvincibleDuration;
        if (invDuration > 0f)
        {
            isInvincible = true;
            invincibleTimer = invDuration;
            spriteFlash.StartInvincibleBlink();
        }

        // 4. 넉백 은 일단 보류
        //move.StartKnockbackByFacing();
    }
    void InterruptOnHit()
    {
        
    }

    // 공격자 위치를 아는 버전
    public void TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        hp.TakeDamage(amount);
        // 넉백 은 일단 보류
        //move.StartKnockbackFromAttacker(attackerWorldPosition);
    }

    void HandleDie()
    {
        playerMove.HandleDieMotion();

        Destroy(gameObject, 2f);
    }

    public void UsePlayerSprite()
    {
        playerSprite.enabled = true;
        playerPartSprite.SetActive (false);
    
    }
    public void UsePartSprite()
    {
        playerSprite.enabled = false;
        playerPartSprite.SetActive (true);

    }
}

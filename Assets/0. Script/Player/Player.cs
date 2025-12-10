using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    HP hp;
    public HP HP { get { return hp; } }

    Exp exp;
    public Exp Exp { get { return exp; } }

    PlayerStats stats;
    public PlayerStats Stats { get { return stats; } }

    public PlayerMove playerMove;
    PlayerAttack playerAttack;

    public SpriteRenderer playerSprite;
    public GameObject playerPartSprite;

    Animator anim;
    
    SpriteFlash spriteFlash;

    public bool CanControl
    {
        get
        {
            // 필요하면 isDead, 컷신 등 같이 묶어서 처리
            return !isStunned && !isDodging;
        }
    }


    void Awake()
    {
        hp = GetComponent<HP>();
        exp = GetComponent<Exp>();
        stats = GetComponent<PlayerStats>();
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        anim = GetComponent<Animator>();
        spriteFlash = GetComponent<SpriteFlash>();
        
    }

    void OnEnable()
    {
        hp.OnDied += HandleDie;
        InputManager.Instance.DodgePressed += OnDodgePressed;
    }

    void OnDisable()
    {
        hp.OnDied -= HandleDie;
        InputManager.Instance.DodgePressed -= OnDodgePressed;
    }

    void Update()
    {
        if(TimeManager.IsPaused) return;
        UpdateHitStun();
        UpdateInvincible();
    }

    #region 구르기
    [SerializeField] GameObject dodgeEffectSprite;
    public bool isDodging = false;
    public float dodgeDuration = 1f;
    public float dodgeCooldown = 2f;
    float nextDodgeAvailableTime = 0f;
    Coroutine dodgeRoutine;
    void OnDodgePressed()
    {
        //회피 가능 조건 확인
        if (TimeManager.IsPaused) return;
        if (!CanControl) return;
        if (Time.time < nextDodgeAvailableTime) return;
        if (isDodging) return;

        playerAttack.EndAttack();

        // 상태 관리
        isDodging = true;

        // 애니메이션, 이펙트 관리
        anim.SetTrigger("Dodge");
        dodgeEffectSprite.SetActive(true);

        // 회피 시작(단순히 회피 시간만큼 기다리기)
        // 회피 속도(물리) 적용은 PlayerMove에서 isDodging 상태 보고 알아서 적용함.
        if (dodgeRoutine != null) StopCoroutine(dodgeRoutine);
        dodgeRoutine = StartCoroutine(DodgeRoutine());
    }

    System.Collections.IEnumerator DodgeRoutine()
    {
        // 회피가 끝나는 시간
        float dodgeEndTime = Time.time + dodgeDuration;
        // 쿨타임이 끝나는 시간
        nextDodgeAvailableTime = Time.time + dodgeCooldown;

        // 회피가 끝나는 시간까지 기다리기
        while (Time.time < dodgeEndTime)
        {
            // 중간에 죽었으면 그냥 끝
            if (HP.IsDead) yield break;
            yield return null;
        }

        EndDodge();
        dodgeRoutine = null;
    }

    void EndDodge()
    {
        isDodging = false;
        anim.SetTrigger("OnDodgeEnd");
        dodgeEffectSprite.SetActive(false);
        playerMove.EndDodge();
    }
    #endregion

    //Player
    #region 착지 공격
    [Header("Air Down Attack")]
    [SerializeField] float airDownPrepareDuration = 0.15f;

    public bool isAirDownAttack;
    public bool isAirDownPrepare;
    float airDownPrepareEndTime;
    public void ExecuteAirDownAttack()
    {
        // 착지공격동안 무적
        isInvincible = true;

        // 이거 보고 PlayerMove가 알아서 무빙 처리
        isAirDownAttack = true;
        isAirDownPrepare = true;

        airDownPrepareEndTime = Time.time + airDownPrepareDuration;
        StartCoroutine(AirDownRoutine());
    }

    IEnumerator AirDownRoutine()
    {
        // 준비 시간 동안 대기
        while (Time.time < airDownPrepareEndTime)
        {
            yield return null;
        }

        // 준비 끝
        isAirDownPrepare = false;

        // PlayerMove에서 낙하를 처리 중이므로, 착지까지 기다리기
        while (!playerMove.isGrounded)
        {
            yield return null;
        }

        // 착지 순간 도착
        EndAirDownAttack();
    }
    
    [Header("Air Down Attack Camera Shake")]
    [SerializeField] float cameraShakeAmplitude;
    [SerializeField] float cameraShakeFrequency;
    [SerializeField] float cameraShakeDuration;
    void EndAirDownAttack()
    {
        isInvincible = false;
        isAirDownAttack = false;
        anim.ResetTrigger("Land");
        CameraManager.Instance.Shake(cameraShakeAmplitude, cameraShakeFrequency, cameraShakeDuration);
    }
    #endregion

    [Header("피격 경직 시간")]
    [SerializeField] float hitStunDuration = 0.15f;
    float stunTimer;
    bool isStunned;
    public bool IsStunned => isStunned;

    void StartHitStun()
    {
        // 경직 적용
        float stunDuration = hitStunDuration;
        if (stunDuration > 0f)
        {
            isStunned = true;
            stunTimer = stunDuration;
            // TODO : 경직 모션 추가
        }
    }
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
    public bool isInvincible;
    public bool IsInvincible => isInvincible;

    void StartInvincible()
    {
        // 피격 후 무적
        float invDuration = hitInvincibleDuration;
        if (invDuration > 0f)
        {
            isInvincible = true;
            invincibleTimer = invDuration;
            spriteFlash.StartInvincibleBlink();
        }
    }

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

    public void Heal(float amount)
    {
        hp.Heal(amount);
    }

    // IDamageable 기본 버전 (공격자 위치 모를 때)
    public void TakeDamage(float amount)
    {
        if (isInvincible) return; //경직 무적
        if (isDodging) return; //구르기 무적

        hp.TakeDamage(amount);

        StartHitStun();
        StartInvincible();

        //spriteFlash.PlayHitFlash();

        // 4. 넉백 은 일단 보류
        //move.StartKnockbackByFacing();
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
        // + 기타 사망 연출
        Destroy(gameObject, 2f);
    }

    //사망 후 처리 (애니메이션 프레임 이벤트로 호출)
    public void OnEndDieAnimation()
    {
        SceneLoader.LoadScene("Start");
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

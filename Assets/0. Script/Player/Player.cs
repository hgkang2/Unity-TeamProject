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
    LocalSFX sfx;

    public bool CanControl
    {
        get
        {
            if (HP.IsDead) return false;
            if (isStunned) return false;
            if (isDodging) return false;
            return true;
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
        sfx = GetComponent<LocalSFX>();
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
    }

    #region 넉백
    public void ApplyKnockback(float force, Vector2? attackerPos = null)
    {
        playerAttack.EndAttack();
        playerMove.ApplyKnockbackImpulse(force, attackerPos);
        anim.SetTrigger("Hit");
    }
    #endregion

    #region 회피
    [SerializeField] GameObject dodgeEffectSprite;
    public bool isDodging = false;
    public bool Dodgeflag = false;
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
        Dodgeflag = true; // 중력 0으로 1회 초기화(PlayerMove.ApplyVelocity())
        BeginInvincible();

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
        EndInvincible();
        anim.SetTrigger("OnDodgeEnd");
        dodgeEffectSprite.SetActive(false);
    }
    #endregion

    #region 착지 공격
    [Header("Air Down Attack")]
    [SerializeField] float airDownPrepareDuration = 0.15f;

    public bool isAirDownAttack;
    public bool isAirDownPrepare;
    float airDownPrepareEndTime;
    public void ExecuteAirDownAttack()
    {
        // 착지공격동안 무적
        BeginInvincible();

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
        anim.SetInteger("AttackType", 4);
        while (!playerMove.isGrounded)
        {
            yield return null;
        }

        // 착지 후 딜레이
        VFXManager.Instance.PlayAttackSpriteVFX("ImpactWave", transform, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.25f);;

        // 공격 끝
        EndAirDownAttack();
    }
    
    [Header("Air Down Attack Camera Shake")]
    [SerializeField] float cameraShakeAmplitude;
    [SerializeField] float cameraShakeFrequency;
    [SerializeField] float cameraShakeDuration;
    void EndAirDownAttack()
    {
        isAirDownAttack = false;
        playerAttack.EndAttack();
        EndInvincible();
        anim.ResetTrigger("Land");
        anim.SetTrigger("AttackEnd");
        CameraManager.Instance.Shake(cameraShakeAmplitude, cameraShakeFrequency, cameraShakeDuration);
    }
    #endregion

    #region 경직
    [Header("피격 경직 시간")]
    [SerializeField] float hitStunDuration = 0.15f;
    bool isStunned;
    public bool IsStunned => isStunned;
    Coroutine stunCoroutine;

    void StartHitStun(float duration)
    {
        // 이미 경직 중이라면 리셋
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        // 경직 적용
        if (hitStunDuration > 0f)
        {
            stunCoroutine = StartCoroutine(HitStunRoutine(duration));
        }
    }

    IEnumerator HitStunRoutine(float duration)
    {
        isStunned = true;
        //anim.SetTrigger("Stun");

        yield return new WaitForSeconds(duration);

        isStunned = false;
        stunCoroutine = null;
    }
    #endregion

    #region 피격 무적
    [Header("피격 무적 시간")]
    [SerializeField] float hitInvincibleDuration = 0.5f;

    public bool isInvincible;
    public bool IsInvincible => isInvincible;

    Coroutine invincibleCoroutine;

    // 무적 시작 (시간 제한 없음, 수동으로 EndInvincible 호출해야 종료)
    public void BeginInvincible()
    {
        if (isInvincible) return;
            
        // 혹시 이전에 돌던 제한시간 코루틴이 있으면 정리
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
        }

        isInvincible = true;
    }

    // 무적 종료 (수동 종료/코루틴 종료 둘 다 여기 사용)
    public void EndInvincible()
    {
        if (!isInvincible && invincibleCoroutine == null) return;
            
        isInvincible = false;

        // 수동 종료 시, 혹시 남아 있을 수 있는 코루틴도 끊기
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
        }
    }

    // 인스펙터에 설정된 기본 시간만큼 무적 유지
    public void StartInvincibleForDuration()
    {
        StartInvincibleForDuration(hitInvincibleDuration);
    }

    // 지정한 시간 동안만 무적 유지 (Begin/End 내부적으로 사용)
    public void StartInvincibleForDuration(float duration)
    {
        if (duration <= 0f)
        {
            EndInvincible();
            return;
        }

        // 기존 제한시간 코루틴이 있으면 리셋
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
        }

        // 무적 시작
        BeginInvincible();
        invincibleCoroutine = StartCoroutine(InvincibleDurationRoutine(duration));
    }

    IEnumerator InvincibleDurationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 코루틴 참조 먼저 비우고
        invincibleCoroutine = null;

        // 그리고 무적 종료
        EndInvincible();
    }
    #endregion

    public void Heal(float amount)
    {
        hp.Heal(amount);
    }

    #region TakeDamage
    public void TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition = null)
    {
        // --- 방어 조건 ---
        if (isInvincible) return;  // 무적
        if (HP.IsDead) return;     // 사망 시 무시

        // --- 데미지 반영 ---
        hp.TakeDamage(amount);
        if(hp.IsDead) return;

        // --- 리액션 처리 ---
        switch (type)
        {
            case DamageType.Normal:
                ApplyKnockback(10f, attackerWorldPosition);
                sfx.Play("Hit");
                goto case DamageType.Area;
            case DamageType.Area:
                StartHitStun(0.15f);
                StartInvincibleForDuration();
                spriteFlash.StartInvincibleBlink(0.5f);
                break;
        }
    }
    #endregion

    void HandleDie()
    {
        playerMove.HandleDieMotion();
        sfx.Play("Die");
        // + 기타 사망 연출
        Destroy(gameObject, 2f);
    }

    //사망 후 처리 (애니메이션 프레임 이벤트로 호출)
    public void OnEndDieAnimation()
    {
        SceneLoader.LoadScene("Start");
        playerSprite.enabled = false;
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

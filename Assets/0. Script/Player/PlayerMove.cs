using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMove : MonoBehaviour
{
    Player player;
    PlayerAttack playerAttack;
    PlayerStats stats;
    Rigidbody2D rb;
    Collider2D col;
    Animator anim;
    SpriteRenderer spr;

    // ---- 입력 / 상태 ----
    public Vector2 inputVec;
    public bool isGrounded = true;
    public bool jumpRequested;

    // ---- 구르기 ----
    public bool isDodging;
    public bool IsDodging => isDodging;
    float dodgeEndTime;
    float cooldownEndTime;
    public readonly float dodgeTime = 0.3f;
    public readonly float dodgeCooldown = 1f;

    // ---- 넉백 ----
    public bool isKnockback;
    public bool IsKnockback => isKnockback;
    Coroutine knockRoutine;
    readonly float knockDur = 0.15f;
    readonly float knockH = 8f;
    readonly float knockV = 3f;

    // ---- 중력 ----
    float baseGrav;
    public float apexGrav = 0.1f;
    public float apexThreshold = 0.7f;
    public float fallGravityMultiplier = 2.0f; // 2~3 정도

    // 공중 제어 보간 속도
    [SerializeField] float airControlLerp = 8f;

    // ---- Ground Check ----
    public LayerMask groundMask;    // 인스펙터에서 "Ground" 레이어 할당
    public float groundRayLength = 0.2f; // 레이 길이
    public float groundRayOffsetX = 0.25f; // 좌우로 얼마나 벌려 쏠지
    // --- 점프 횟수 관리 ---
    public int maxJumpCount = 2;
    int currentJumpCount;

    void Awake()
    {
        player = GetComponent<Player>();
        playerAttack = GetComponent<PlayerAttack>();
        stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        baseGrav = rb.gravityScale;
        currentJumpCount = maxJumpCount;
    }

    void OnEnable()
    {
        InputManager.Instance.JumpPressed += OnJumpPressed;
        InputManager.Instance.DodgePressed += OnDodgePressed;
    }

    void OnDisable()
    {
        InputManager.Instance.JumpPressed -= OnJumpPressed;
        InputManager.Instance.DodgePressed -= OnDodgePressed;
    }
    // -------------------------------
    // Update : 입력 및 상태 판단
    // -------------------------------
    void Update()
    {
        if (player.HP.IsDead) return;

        ReadInput();

        if (isKnockback) return;

        if (isDodging)
        {
            if (Time.time >= dodgeEndTime) EndDodge();
            return;
        }
    }

    // -------------------------------
    // FixedUpdate : 물리 적용
    // -------------------------------
    void FixedUpdate()
    {
        if (player.HP.IsDead) return;

        bool external = isDodging || isKnockback;
        if (!external)
        {
            HandleJump();
            HandleMove();
        }

        HandleGroundCheck();
        GetGroundDistance();
        HandleGravity();
    }

    // -------------------------------
    // LateUpdate : 시각/애니메이션 정리
    // -------------------------------
    void LateUpdate()
    {
        if (PauseManager.IsPaused) return;

        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        anim.SetFloat("Move", Mathf.Abs(rb.linearVelocity.x));

        bool external = isDodging || isKnockback;
        if (!external && inputVec.x != 0)
            spr.flipX = inputVec.x < 0;
    }

    // ---- 입력 ----
    void ReadInput()
    {
        inputVec = InputManager.Instance.Move;
    }
    void OnJumpPressed()
    {
        if (!isDodging && !isKnockback && (isGrounded || currentJumpCount > 0))
        {
            jumpRequested = true;
        }
    }
        // ---- 이동 / 점프 ----
        void HandleMove()
    {
        // 1) 공중 + 공격 중 → 입력 무시, 관성 유지
        if (playerAttack.isAttacking && !isGrounded)
        {
            // 공중 공격 중에는 입력 무시, 현재 속도 유지
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);
            return;
        }

        // 2) 지상 + 공격 중 → 제자리 공격
        if (playerAttack.isAttacking && isGrounded)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // 3) 일반 이동
        float targetX = inputVec.x * stats.curMoveSpeed;
        float newX = targetX;

        // 공중일 땐 방향 전환이 완만하게(Lerp)
        if (!isGrounded)
        {
            newX = Mathf.Lerp(rb.linearVelocity.x, targetX, Time.fixedDeltaTime * airControlLerp);
        }

        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        if (!jumpRequested) return;
        jumpRequested = false;

        bool canJump = isGrounded || currentJumpCount > 0;
        if (!canJump)return;
        anim.ResetTrigger("Land");
        
        if (isGrounded)
        {
            currentJumpCount = maxJumpCount -1;
        }
        else
        {
            currentJumpCount--;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * stats.curJumpForce, ForceMode2D.Impulse);
        isGrounded = false;
        anim.SetTrigger("Jump");
        anim.SetBool("IsJumping", true);
    }

    // ---- 구르기 ----
    void OnDodgePressed()
    {
        if (Time.time >= cooldownEndTime && !isKnockback && !player.HP.IsDead)
        {
            StartDodge();
        }
    }

    void StartDodge()
    {
        isDodging = true;
        dodgeEndTime = Time.time + dodgeTime;
        cooldownEndTime = Time.time + dodgeCooldown;

        anim.SetTrigger("Dodge");
        float dir = spr.flipX ? -1f : 1f;

        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(dir * stats.curMoveSpeed * 2f, 0);
    }

    void EndDodge()
    {
        isDodging = false;
        rb.gravityScale = baseGrav;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void ForceStopDodge()
    {
        if (isDodging) EndDodge();
    }

    // ---- 넉백 ----
    public void StartKnockbackByFacing()
    {
        Vector2 dir = spr.flipX ? Vector2.right : Vector2.left;
        StartKnockback(dir);
    }

    public void StartKnockbackFromAttacker(Vector2 attackerPos)
    {
        Vector2 dir = rb.position - attackerPos;
        if (dir.sqrMagnitude < 0.001f)
            dir = spr.flipX ? Vector2.right : Vector2.left;
        StartKnockback(dir.normalized);
    }

    void StartKnockback(Vector2 dir)
    {
        if (isDodging) ForceStopDodge();
        if (knockRoutine != null) StopCoroutine(knockRoutine);
        knockRoutine = StartCoroutine(Knock(dir));
    }

    System.Collections.IEnumerator Knock(Vector2 dir)
    {
        isKnockback = true;
        spr.flipX = dir.x > 0.01f; // 오른쪽으로 밀리면 왼쪽 봄

        rb.gravityScale = baseGrav;
        rb.linearVelocity = new Vector2(dir.x * knockH, knockV);

        float t = 0;
        while (t < knockDur) { t += Time.deltaTime; yield return null; }
        isKnockback = false;
    }

    // ---- 중력 / 착지 ----
    void HandleGravity()
    {
        if (isDodging || isKnockback)
        {
            rb.gravityScale = baseGrav;
            return;
        }

        if (!isGrounded)
        {
            if (rb.linearVelocity.y < 0) // 떨어지는 중
                rb.gravityScale = baseGrav * fallGravityMultiplier;
            else                         // 올라가는 중
                rb.gravityScale = baseGrav;
        }
        else
        {
            rb.gravityScale = baseGrav;
        }
    }



    // ---- 죽음 모션 ----
    public void HandleDieMotion()
    {
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void HandleGroundCheck()
    {
        Bounds b = col.bounds;

        // 좌하, 우하에서 레이 쏘기
        Vector2 leftOrigin = new Vector2(b.min.x + groundRayOffsetX, b.min.y + 0.05f);
        Vector2 rightOrigin = new Vector2(b.max.x - groundRayOffsetX, b.min.y + 0.05f);

        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, groundRayLength, groundMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, groundRayLength, groundMask);

        bool hitSomething = (hitLeft.collider != null) || (hitRight.collider != null);

        bool wasGrounded = isGrounded;
        bool groundedNow = hitSomething;
        isGrounded = groundedNow;

        bool landedThisFrame = !wasGrounded && groundedNow && rb.linearVelocityY < -0.01;


        if (landedThisFrame)
        {
            currentJumpCount = maxJumpCount;
            anim.SetBool("IsJumping", false);

            // 공중 공격 상태로 내려온 경우엔 Land 스킵
            if (!playerAttack.isAttacking)
            {
                anim.SetTrigger("Land");
            }
            else
            {
                anim.ResetTrigger("Land");
            }
        }

        isGrounded = groundedNow;

#if UNITY_EDITOR
        Color c = groundedNow ? Color.green : Color.red;
        Debug.DrawRay(leftOrigin, Vector2.down * groundRayLength, c);
        Debug.DrawRay(rightOrigin, Vector2.down * groundRayLength, c);
#endif
    }

    public float minGroundDistanceForAirAttack; // 이보다 낮으면 공중공격 금지

    public float GetGroundDistance()
    {
        Bounds b = col.bounds;
        Vector2 leftOrigin = new Vector2(b.min.x + groundRayOffsetX, b.min.y + 0.05f);
        Vector2 rightOrigin = new Vector2(b.max.x - groundRayOffsetX, b.min.y + 0.05f);

        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, minGroundDistanceForAirAttack, groundMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, minGroundDistanceForAirAttack, groundMask);

        float dist = -1f;

        if (hitLeft.collider != null)
            dist = hitLeft.distance;

        if (hitRight.collider != null)
        {
            if (dist < 0f || hitRight.distance < dist)
                dist = hitRight.distance;
        }

#if UNITY_EDITOR
        Color c = dist >= 0f ? Color.cyan : Color.gray;
        Debug.DrawRay(leftOrigin, Vector2.down * minGroundDistanceForAirAttack, c);
        Debug.DrawRay(rightOrigin, Vector2.down * minGroundDistanceForAirAttack, c);
#endif

        return dist; // -1이면 바닥 없음
    }


}

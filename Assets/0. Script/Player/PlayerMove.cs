using System;
using UnityEngine;

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
    public bool isGrounded = false;
    public bool isRightFacing = true;
    public bool IsWalking
    {
        get
        {
            if (!isGrounded) return false;
            if (Mathf.Abs(rb.linearVelocity.x) < 0.1f) return false;
            if (!player.CanControl) return false;
            if (isWallGrabbing || isWallSliding) return false;
            return true;
        }
    }

    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.12f;
    float lastGroundedTime = -999f;
    float lastJumpPressedTime = -999f;


    // 공중 제어 보간 속도
    [SerializeField] float airControlLerp = 8f;

    // ---- Ground Check ----
    public LayerMask groundMask;    // 인스펙터에서 "Ground" 레이어 할당
    public float groundRayLength = 0.2f; // 레이 길이
    public float groundRayOffsetX = 0.25f; // 좌우로 얼마나 벌려 쏠지


    #region 초기화(awake)
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
    }

    void OnDisable()
    {
        InputManager.Instance.JumpPressed -= OnJumpPressed;
    }
    #endregion

    // -------------------------------
    // Update : 입력 및 상태 판단
    // -------------------------------
    void Update()
    {
        if (TimeManager.IsPaused) return;
        if (player.HP.IsDead) return;

        //경직 시 행동 불가(입력 막기)
        if (!player.CanControl)
        {
            inputVec = Vector2.zero;
            return;
        }

        // 플레이어가 입력 가능할 때만 방향 바꿀 수 있도록
        if (player.CanControl && Mathf.Abs(inputVec.x) > 0.1f)
        {
            isRightFacing = inputVec.x > 0;
        }


        ReadInput();
    }

    // -------------------------------
    // FixedUpdate : 물리 적용
    // -------------------------------
    void FixedUpdate()
    {
        if (player.HP.IsDead) return;

        HandleWallCheck();
        HandleWallAction();

        bool external = player.isDodging || !player.CanControl || player.isAirDownAttack || isWallGrabbing || isWallSliding;
        if (!external)
        {
            HandleJump();
            HandleMove();
        }
        else
        {
            // 제어 불가 상태에서 자체 이동 효과
            if (player.isDodging) DodgeMovement();
            else if (player.isAirDownAttack) AirDownMovement();
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
        if (TimeManager.IsPaused) return;

        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        anim.SetFloat("Move", Mathf.Abs(rb.linearVelocity.x));

        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsWallGrabbing", isWallGrabbing || isWallSliding);
        if (playerAttack.isAttacking || isWallGrabbing || isWallSliding) return;
        //방향 바꾸기
        float dir = isRightFacing ? 1 : -1;
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
    }

    // ---- 입력 ----
    void ReadInput()
    {
        inputVec = InputManager.Instance.Move;
    }
    void OnJumpPressed()
    {
        if (player.isDodging) return;
        lastJumpPressedTime = Time.time;
    }
    #region 이동
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
    #endregion

    #region 점프
    public int maxJumpCount = 2;
    int currentJumpCount;
    void HandleJump()
    {
        // 1) 점프 선입력 버퍼 안에 있는지 확인
        bool hasBufferedJump = (Time.time - lastJumpPressedTime) <= jumpBufferTime;
        if (!hasBufferedJump)
        {
            return;
        }

        // 2) 코요테 타임 안이면 "지상 점프 가능"으로 본다
        bool withinCoyote = (Time.time - lastGroundedTime) <= coyoteTime;
        bool canUseGroundJump = isGrounded || withinCoyote;

        // 3) 그게 아니면 공중 점프(이단 점프)만 검사
        bool canUseDoubleJump = !canUseGroundJump && currentJumpCount > 0;

        if (!canUseGroundJump && !canUseDoubleJump) return;

        // 여기까지 왔으면 버퍼를 소비(한 번만 쓰고 버림)
        lastJumpPressedTime = -999f;

        // 4) 벽 점프 우선
        if (isWallGrabbing || isWallSliding)
        {
            HandleWallJump();
            return;
        }

        anim.ResetTrigger("Land");

        if (canUseGroundJump)
        {
            // 지상(코요테 포함)에서 점프하면 남은 점프 횟수 세팅
            currentJumpCount = maxJumpCount - 1;
            anim.SetTrigger("Jump");
        }
        else
        {
            // 공중 점프 소모
            currentJumpCount--;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * stats.curJumpForce, ForceMode2D.Impulse);
        isGrounded = false;
        anim.SetTrigger("Jump");
        anim.SetBool("IsJumping", true);
    }


    void HandleWallJump()
    {
        isWallGrabbing = false;
        isWallSliding = false;
        rb.gravityScale = baseGrav;
        float jumpDir = isRightFacing ? -1 : 1;
        isRightFacing = !isRightFacing;
        rb.linearVelocity = Vector2.zero;
        Vector2 wallJumpVec = new Vector2(wallJumpForceX * jumpDir, wallJumpForceY);
        rb.AddForce(wallJumpVec, ForceMode2D.Impulse);
        currentJumpCount--;
    }
    #endregion

    #region 넉백
    public void ApplyKnockbackImpulse(float force, Vector2? attackerPos = null)
    {
        // 기존 속도 리셋
        rb.linearVelocity = Vector2.zero;

        Vector2 dir;

        if (attackerPos.HasValue)
        {
            // 맞은 방향의 반대쪽
            dir = ((Vector2)transform.position - attackerPos.Value).normalized;
        }
        else
        {
            // 공격자 정보가 없으면 바라보는 반대쪽으로 튕기기
            float dirX = isRightFacing ? -1f : 1f;
            dir = new Vector2(dirX, 0.3f).normalized;
        }

        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }
    #endregion

    #region 회피
    void DodgeMovement()
    {
        rb.gravityScale = 0;
        float dir = isRightFacing ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * stats.curMoveSpeed * 2f, 0);
    }

    public void EndDodge()
    {
        rb.gravityScale = baseGrav;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    #endregion

    #region  착지 공격
    [SerializeField] float airDownFallSpeed = 20f;
    [SerializeField] float airDownGravityScale = 5f;
    void AirDownMovement()
    {
        // 착지 준비 시간 동안 멈춰 있기
        if (player.isAirDownPrepare)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        // 착지 준비가 끝나면 땅에 내려꽂히기
        else
        {
            rb.gravityScale = airDownGravityScale;
            rb.linearVelocity = new Vector2(0f, -airDownFallSpeed);
        }
    }
    #endregion

    #region 중력
    float baseGrav = 9.81f;
    public float apexGrav = 0.1f;
    public float apexThreshold = 0.7f;
    public float fallGravityMultiplier = 2.0f; // 2~3 정도
    void HandleGravity()
    {
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
    #endregion

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

        if (groundedNow)
        {
            lastGroundedTime = Time.time;
        }

        isGrounded = groundedNow;

        bool landedThisFrame = !wasGrounded && groundedNow && rb.linearVelocityY < -0.01;


        if (landedThisFrame)
        {
            currentJumpCount = maxJumpCount;

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

    #region 벽타기
    public bool isWallGrabbing = false;
    public bool isWallSliding = false;
    public LayerMask wallMask;
    [SerializeField] float wallCheckDistance = 0.5f;
    [SerializeField] float wallSlideSpeed = 2f;
    [SerializeField] float wallJumpForceX = 10f;
    [SerializeField] float wallJumpForceY = 15f;
    void HandleWallCheck()
    {
        if (isGrounded || player.isDodging)
        {
            isWallGrabbing = false;
            isWallSliding = false;
            return;
        }
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = isRightFacing ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, wallCheckDistance, wallMask);
        bool isTouchingWall = (hit.collider != null);
        bool inputTowardWall = (isRightFacing && inputVec.x > 0.01f) || (!isRightFacing && inputVec.x < -0.01f);
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            if (inputTowardWall)
            {
                isWallGrabbing = true;
                isWallSliding = false;
            }
            else
            {
                isWallSliding = true;
                isWallGrabbing = false;
            }
        }
        else
        {
            isWallGrabbing = false;
            isWallSliding = false;
        }
    }
    float originGravityScale;
    void HandleWallAction()
    {
        if (isWallGrabbing)
        {
            if (rb.gravityScale != 0f)
            {
                originGravityScale = rb.gravityScale;
                rb.gravityScale = 0f;
            }
            rb.linearVelocity = Vector2.zero;
            currentJumpCount = maxJumpCount;
        }
        else if (isWallSliding)
        {
            if (rb.gravityScale != originGravityScale)
            {
                rb.gravityScale = originGravityScale;
            }
            rb.linearVelocity = new Vector2(0f, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
            currentJumpCount = maxJumpCount;
        }
        else
        {
            if (rb.gravityScale != baseGrav && !player.isDodging && !player.isAirDownAttack)
            {
                rb.gravityScale = baseGrav;
            }
        }
    }
    #endregion

    // ---- 죽음 모션 ----
    public void HandleDieMotion()
    {
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    


}

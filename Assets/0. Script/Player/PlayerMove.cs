using System;
using Unity.VisualScripting;
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
    LocalSFX sfx;

    [Header("입력/상태")]
    public Vector2 inputVec;
    public bool isGrounded = false;
    public bool isRightFacing = true;
    public GravityMode gravityMode = GravityMode.Normal;
    public bool IsWalking
    {
        get
        {
            if (!isGrounded) return false;
            if (Mathf.Abs(rb.linearVelocity.x) < 0.1f) return false;
            if (!player.CanControl) return false;
            return true;
        }
    }

    [Header("점프 조작")]
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.12f;
    [Header("공중 조작 감쇠")]
    [SerializeField] float airControlLerp = 8f;
    float lastGroundedTime = -999f;
    float lastJumpPressedTime = -999f;


    

    public event Action JumpCommitted;


    void Awake()
    {
        player = GetComponent<Player>();
        playerAttack = GetComponent<PlayerAttack>();
        stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        sfx = GetComponent<LocalSFX>();

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

    void Update()
    {
        if (TimeManager.IsPaused) return;
        if (player.HP.IsDead) return;

        // 플레이어 조작 불가 상태 or 벽점프후 0.25초 이내
        if (!player.CanControl || Time.unscaledTime < wallJumpTime + 0.25f)
        {
            inputVec = Vector2.zero;
            return;
        }

        ReadInput();

        if (player.CanControl && Mathf.Abs(inputVec.x) > 0.1f)
        {
            isRightFacing = inputVec.x > 0;
        }
    }

    void FixedUpdate()
    {
        if (player.HP.IsDead) return;

        // 1) 센서/상태 업데이트
        HandleWallCheck();
        HandleGroundCheck();
        GetGroundDistance();

        // 2) 점프
        bool canNormalControl = player.CanControl && !player.isAirDownAttack;
        if (canNormalControl)
        {
            HandleJump();
        }

        // 3) 중력 적용
        UpdateGravityMode();
        ApplyGravity();

        // 4) 좌우 이동 적용(필요시 상하이동 제한)
        ApplyVelocity();

        mygravity = rb.gravityScale;
    }
    public float mygravity;

    void LateUpdate()
    {
        if (TimeManager.IsPaused) return;

        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        anim.SetFloat("InputX", Mathf.Abs(inputVec.x));
        anim.SetFloat("MoveX", Mathf.Abs(rb.linearVelocity.x));

        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsWallGrabbing", isWallGrabbing || isWallSliding);

        if (playerAttack.isAttacking || isWallGrabbing || isWallSliding) return;

        float dir = isRightFacing ? 1 : -1;
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;
    }

    void ReadInput()
    {
        inputVec = InputManager.Instance.Move;
    }

    void OnJumpPressed()
    {
        if (!player.CanControl) return;
        if (playerAttack.isAttacking) return;
        lastJumpPressedTime = Time.time;
    }

    float wallJumpTime;

    #region 속도 적용
    public void StopMoveOnce()
    {
        rb.linearVelocity = Vector2.zero;
    }
    void ApplyVelocity()
    {
        float newX = rb.linearVelocity.x;
        float newY = rb.linearVelocity.y;
        // 벽 잡기
        if (isWallGrabbing)
        {
            newX = 0f;
            newY = 0f;
            currentJumpCount = maxJumpCount;
        }
        // 벽 슬라이드
        else if (isWallSliding)
        {
            newX = 0f;
            currentJumpCount = maxJumpCount;
            if (!wasWallSliding) newY = 0f; // 슬라이드 시작시에 1회 초기화
        }
        // 회피
        else if (player.isDodging)
        {
            float dir = isRightFacing ? 1f : -1f;
            newX = dir * stats.curMoveSpeed * 2f;
            if (player.Dodgeflag)
            {
                player.Dodgeflag = false;
                newY = 0f;
            }
        }
        // 내려찍기
        else if (player.isAirDownAttack)
        {
            newX = 0f;
            if (player.isAirDownPrepare) newY = 0f;
        }
        // 지상 공격 중 이동불가
        else if (playerAttack.isAttacking && isGrounded)
        {
            newX = 0f;
        }
        // 일반 이동
        else
        {
            float targetX = inputVec.x * stats.curMoveSpeed;
            newX = isGrounded
                ? targetX
                : Mathf.Lerp(rb.linearVelocity.x, targetX, Time.fixedDeltaTime * airControlLerp);
            if(Time.time < wallJumpTime + 0.25f)
            {
                newX = rb.linearVelocityX;
            }
            // 발끝이 벽에 걸리면 움직이지 않기
            if (!isGrounded && isFootTouchingWall && !isWallGrabbing && !isWallSliding)
            {
                bool inputTowardWall =
                    (isRightFacing && inputVec.x > 0.1f) ||
                    (!isRightFacing && inputVec.x < -0.1f);

                if (inputTowardWall)
                {
                    newX = 0f;
                }
            }
        }

        // 최종 적용
        rb.linearVelocity = new Vector2(newX, newY);
    }

    #endregion

    #region Jump/WallJump
    [Header("최대 점프 가능 횟수")]
    public int maxJumpCount = 2;
    [Header("남은 점프 가능 횟수")]
    public int currentJumpCount;

    void HandleJump()
    {
        bool hasBufferedJump = (Time.time - lastJumpPressedTime) <= jumpBufferTime;
        if (!hasBufferedJump) return;

        bool withinCoyote = (Time.time - lastGroundedTime) <= coyoteTime;
        bool canUseGroundJump = isGrounded || withinCoyote;

        bool canUseDoubleJump = !canUseGroundJump && currentJumpCount > 0;
        if (!canUseGroundJump && !canUseDoubleJump) return;

        lastJumpPressedTime = -999f;

        if (isWallGrabbing || isWallSliding)
        {
            HandleWallJump();
            return;
        }

        anim.ResetTrigger("Land");

        if (canUseGroundJump)
        {
            currentJumpCount = maxJumpCount - 1;
        }
        else
        {
            currentJumpCount--;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * stats.curJumpForce, ForceMode2D.Impulse);

        isGrounded = false;
        anim.SetTrigger("Jump");
        sfx.Play("Jump");
        JumpCommitted?.Invoke();
    }
    [Header("벽 점프시 반대쪽으로 튕기는 힘")]
    public bool canWallJump = false;
    public float wallJumpForceX = 5;
    void HandleWallJump()
    {
        if(!canWallJump) return;
        
        isWallGrabbing = false;
        isWallSliding = false;

        float jumpDir = isRightFacing ? -1f : 1f;
        isRightFacing = !isRightFacing;

        rb.linearVelocity = Vector2.zero;

        Vector2 wallJumpVec = new Vector2(wallJumpForceX * jumpDir, stats.curJumpForce);
        rb.AddForce(wallJumpVec, ForceMode2D.Impulse);
        sfx.Play("JumpVoice");

        currentJumpCount = 0;
        wallJumpTime = Time.time;
    }
    #endregion

    #region Gravity
    [Header("중력 관련")]
    public float baseGrav = 9.81f;
    public float apexGravityMultiplier = 0.66f; // 점프 정점 중력 계수
    public float apexThreshold = 0.6f; // 점프 정점 구간
    public float fallGravityMultiplier = 2.5f; // 공중에서 떨어질때 가속 계수
    public float wallSlideGravityMultiplier = 0.25f;
    [SerializeField] float airDownGravityScale = 50f; // 내려찍기 중력
    void UpdateGravityMode()
    {
        if (player.isDodging)
        {
            gravityMode = GravityMode.Normal;
            return;
        }
        // --- 우선순위 규칙 ---
        if (isWallGrabbing || player.isAirDownPrepare)
        {
            gravityMode = GravityMode.Zero;
            return;
        }

        if (player.isAirDownAttack)
        {
            gravityMode = GravityMode.AirDown;
            return;
        }

        if (isWallSliding)
        {
            gravityMode = GravityMode.WallSlide;
            return;
        }

        // ---일반 규칙---

        // 지상
        if (isGrounded)
        {
            gravityMode = GravityMode.Normal;
            return;
        }

        // 점프 정점
        if (Mathf.Abs(rb.linearVelocity.y) < apexThreshold)
        {
            gravityMode = GravityMode.Apex;
            return;
        }

        // 떨어지는 중
        if (rb.linearVelocity.y < 0f)
        {
            gravityMode = GravityMode.Falling;
            return;
        }

        gravityMode = GravityMode.Normal;
    }

    // 규칙(모드)에 따른 중력값 적용
    void ApplyGravity()
    {
        float newGrav = baseGrav;

        switch (gravityMode)
        {
            case GravityMode.Zero:
                newGrav = 0f;
                break;

            case GravityMode.Apex:
                newGrav = baseGrav * apexGravityMultiplier;
                break;

            case GravityMode.Falling:
                newGrav = baseGrav * fallGravityMultiplier;
                break;

            case GravityMode.AirDown:
                newGrav = airDownGravityScale;
                break;

            case GravityMode.WallSlide:
                newGrav = baseGrav * wallSlideGravityMultiplier;
                break;
                
            default:
                newGrav = baseGrav;
                break;
        }

        rb.gravityScale = newGrav;
    }
    #endregion

    #region Ground Check
    public LayerMask groundMask;
    [Header("바닥 인식")]
    public float groundRayLength = 0.2f;
    public float groundRayOffsetX = 0.25f;
    void HandleGroundCheck()
    {
        Bounds b = col.bounds;

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

        bool landedThisFrame = !wasGrounded && groundedNow && rb.linearVelocityY < -0.01f;
        if (landedThisFrame)
        {
            currentJumpCount = maxJumpCount;

            if (!playerAttack.isAttacking)
            {
                anim.SetTrigger("Land");
            }
            else
            {
                anim.ResetTrigger("Land");
            }

            sfx.Play("Land");
        }

#if UNITY_EDITOR
        Color c = groundedNow ? Color.green : Color.red;
        Debug.DrawRay(leftOrigin, Vector2.down * groundRayLength, c);
        Debug.DrawRay(rightOrigin, Vector2.down * groundRayLength, c);
#endif
    }

    public float minGroundDistanceForAirAttack;

    public float GetGroundDistance()
    {
        Bounds b = col.bounds;
        Vector2 leftOrigin = new Vector2(b.min.x + groundRayOffsetX, b.min.y + 0.05f);
        Vector2 rightOrigin = new Vector2(b.max.x - groundRayOffsetX, b.min.y + 0.05f);

        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, minGroundDistanceForAirAttack, groundMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, minGroundDistanceForAirAttack, groundMask);

        float dist = -1f;

        if (hitLeft.collider != null) dist = hitLeft.distance;

        if (hitRight.collider != null)
        {
            if (dist < 0f || hitRight.distance < dist) dist = hitRight.distance;
        }

#if UNITY_EDITOR
        Color c = dist >= 0f ? Color.cyan : Color.gray;
        Debug.DrawRay(leftOrigin, Vector2.down * minGroundDistanceForAirAttack, c);
        Debug.DrawRay(rightOrigin, Vector2.down * minGroundDistanceForAirAttack, c);
#endif

        return dist;
    }
    #endregion

    #region  Wall Check
    
    [Header("벽 잡기 상태")]
    public bool isWallGrabbing = false;
    public bool isWallSliding = false;
    [SerializeField] float wallSlideSpeed = 2f;
    
    [Header("벽 잡기 인식")]
    public LayerMask wallMask;
    [SerializeField] float wallCheckDistance = 0.5f;
    [SerializeField] float wallCheckYOffset = 0f;

    [Header("벽 인식")]
    public bool isFootTouchingWall = false;
    [SerializeField] float footWallCheckDistance = 0.08f; // 0.03~0.12 사이로 취향 조절
    [SerializeField] float footWallCheckYOffset = 0.05f;  // b.min.y에서 살짝 위(발끝)
    bool wasWallSliding;
    void HandleWallCheck()
    {
        if (isGrounded || !player.CanControl)
        {
            isWallGrabbing = false;
            isWallSliding = false;
        isFootTouchingWall = false;
            return;
        }

        wasWallSliding = isWallSliding;

        Bounds b = col.bounds;

        // 벽 판정용 중앙 레이
        float rayY = b.center.y + wallCheckYOffset;
        Vector2 rayOrigin = new Vector2(isRightFacing ? b.max.x : b.min.x, rayY);
        Vector2 rayDirection = isRightFacing ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, wallCheckDistance, wallMask);

#if UNITY_EDITOR
        Color color = hit.collider != null ? Color.red : Color.green;
        Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * wallCheckDistance, color);
#endif


        bool isTouchingWall = (hit.collider != null);

        bool inputTowardWall =
            (isRightFacing && inputVec.x > 0.1f) ||
            (!isRightFacing && inputVec.x < -0.1f);

        bool allowWallState = (rb.linearVelocity.y < 0f) || isWallGrabbing || isWallSliding;

        // 중앙 레이가 맞았으면 벽잡기 상태로 진입
        if (isTouchingWall && allowWallState)
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

            isFootTouchingWall = false;
            return;
        }

        // 중앙 레이가 안 맞으면 벽잡기 상태 해제
        else
        {
            isWallGrabbing = false;
            isWallSliding = false;
        }

        // 전방 머리나 하체쪽에 벽이 있을때 스턱 방지용 면 체크
        //Vector2 castOrigin = b.center;
        Vector2 castSize = new Vector2(0.05f, b.size.y * 0.9f);
        Vector2 castOrigin = new Vector2(isRightFacing ? b.max.x : b.min.x, b.center.y);

        hit = Physics2D.BoxCast(
            castOrigin,
            castSize,
            0f,
            rayDirection,
            footWallCheckDistance,
            wallMask
        );

        isFootTouchingWall = (hit.collider != null);

#if UNITY_EDITOR
        Color footColor = isFootTouchingWall ? Color.magenta : Color.cyan;

        // 박스 윤곽 + 캐스트 방향 표시
        Vector2 half = castSize * 0.5f;
        Vector2 p1 = castOrigin + new Vector2(-half.x, -half.y);
        Vector2 p2 = castOrigin + new Vector2(-half.x, half.y);
        Vector2 p3 = castOrigin + new Vector2(half.x, half.y);
        Vector2 p4 = castOrigin + new Vector2(half.x, -half.y);

        Debug.DrawLine(p1, p2, footColor);
        Debug.DrawLine(p2, p3, footColor);
        Debug.DrawLine(p3, p4, footColor);
        Debug.DrawLine(p4, p1, footColor);

        Debug.DrawLine(castOrigin, castOrigin + rayDirection * footWallCheckDistance, footColor);
#endif
    }
    #endregion

    #region 넉백
    public void ApplyKnockbackImpulse(float force, Vector2? attackerPos = null)
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 dir;

        if (attackerPos.HasValue)
        {
            dir = ((Vector2)transform.position - attackerPos.Value).normalized;
        }
        else
        {
            float dirX = isRightFacing ? -1f : 1f;
            dir = new Vector2(dirX, 0.3f).normalized;
        }

        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }
    #endregion

    public void HandleDieMotion()
    {
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
    public void ResetAfterDeath()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        isGrounded = true; 
    }
}

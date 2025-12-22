using System.Globalization;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    Player player;
    PlayerMove playerMove;
    PlayerStats stats;
    Animator animator;
    HitVfx hitVfx;

    //Player 아래에 각 공격 범위 별로 히트박스 만들기 후 
    //4개다 Player-PlayerAttack 인스펙터에 끝어다 넣기(Player 하위에 만들어논거 참고)
    [Header("각 공격별 히트박스")]
    [SerializeField] PlayerHitBox normalHitbox;
    [SerializeField] PlayerHitBox upHitbox;
    [SerializeField] PlayerHitBox jumpHitbox;
    [SerializeField] PlayerHitBox downHitbox;
    //[SerializeField] PlayerHitBox specialHitbox;

    public bool isAttacking;
    AttackType currentType = AttackType.None;


    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        stats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        hitVfx = GetComponent<HitVfx>();
    }
    void Start()
    {
        DisableAllHitboxes();
    }

    void OnEnable()
    {
        normalHitbox.OnHit += HandleHit;
        upHitbox.OnHit += HandleHit;
        jumpHitbox.OnHit += HandleHit;
        downHitbox.OnHit += HandleHit;
        //specialHitbox.OnHit += HandleHit;
        InputManager.Instance.AttackPressed += HandleAttackPressed;
        InputManager.Instance.SpecialAttackPressed += HandleSpecialAttackPressed;
    }
    void OnDisable()
    {
        normalHitbox.OnHit -= HandleHit;
        upHitbox.OnHit -= HandleHit;
        jumpHitbox.OnHit -= HandleHit;
        downHitbox.OnHit -= HandleHit;
        //specialHitbox.OnHit -= HandleHit;
        InputManager.Instance.AttackPressed -= HandleAttackPressed;
        InputManager.Instance.SpecialAttackPressed -= HandleSpecialAttackPressed;
    }

    void HandleHit(Collider2D other)
    {
        Vector2 hitPoint = other.ClosestPoint(transform.position);
        Vector2 normal = (other.transform.position - transform.position).normalized;

        HitContext ctx = new HitContext
        {
            point = hitPoint,
            normal = normal,
            attackerWorldPos = transform.position,
            instigator = gameObject,
            amount = stats.curDamage,
            type = DamageType.Normal
        };

        if (other.TryGetComponent<IHitReceiver>(out var hit))
        {
            hit.OnHit(ctx);
            hitVfx.Play(ctx);
        }

        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(ctx.amount, ctx.type, ctx.attackerWorldPos);
        }

        // 적 타격 시 Hit Stop
        switch (currentType)
        {
            case AttackType.Special:
                // 궁극기 시전시 평타보다 좀 더 느려지게
                // TimeManager.SlowForMoment(0.1f, 0.05f, 0.15f);
                break;
            default:
                // 평타 시전시 약간 느려지게
                TimeManager.SlowForMoment(0, 0, 0.05f);
                break;
        }
    }

    // Attack 키를 눌렀을때 반응
    void HandleAttackPressed()
    {
        if (isAttacking) return;
        if (player != null && !player.CanControl) return;

        // 1. 공중 + 바닥 너무 가까우면 공격 금지
        if (playerMove != null && !playerMove.isGrounded)
        {
            float dist = playerMove.GetGroundDistance();
            if (dist >= 0f && dist < playerMove.minGroundDistanceForAirAttack)
            {
                return;
            }
        }

        // 2. 현재 방향/점프 상태에 따라 AttackType 결정
        Vector2 inputDir = InputManager.Instance.Move;

        AttackType type;

        // 위 방향키
        if (inputDir.y > 0.5f)
        {
            type = AttackType.Up;
        }
        // 아래 방향키 + 공중일경우만
        else if (inputDir.y < -0.5f && !playerMove.isGrounded)
        {
            type = AttackType.Down;
        }
        // 점프 버튼 + 공중일경우만
        else if (InputManager.Instance.IsJumpHeld && !playerMove.isGrounded)
        {
            type = AttackType.Jump;
        }
        else
        {
            type = AttackType.Normal;
        }

        StartAttack(type);
    }

    void HandleSpecialAttackPressed()
    {
        if (isAttacking) return;
        //StartAttack(AttackType.Special);
    }

    public void StartAttack(AttackType type)
    {
        if (type == AttackType.Down)
        {
            player.ExecuteAirDownAttack();
        }

        animator.SetTrigger("Attack");
        animator.SetInteger("AttackType", (int)type);

        isAttacking = true;
    }

    // 각 공격 애니메이션의 첫 프레임에 연결 (꼭 숫자도 채우기)
    // HeroKnight_Attack animation 참고
    public void OnAttackStart(int attackId)
    {
        //isAttacking = true; 이미 위에 있음
        //추후 공격 시작시 필요한 것 추가하기
    }

    // 각 공격 애니메이션의 공격 시작 프레임에 연결
    public void OnHitboxOn(int attackId)
    {
        AttackType type = (AttackType)attackId;
        EnableHitbox(type);
    }

    // 각 공격 애니메이션의 공격 끝 프레임에 연결
    public void OnHitboxOff()
    {
        DisableAllHitboxes();
    }

    // 각 공격 애니메이션의 마지막 프레임에 연결
    public void EndAttack()
    {
        isAttacking = false;
        currentType = AttackType.None;
        DisableAllHitboxes();
    }

    void EnableHitbox(AttackType type)
    {
        DisableAllHitboxes();

        switch (type)
        {
            case AttackType.Normal:
                if (normalHitbox != null) normalHitbox.col.enabled = true;
                break;
            case AttackType.Up:
                if (upHitbox != null) upHitbox.col.enabled = true;
                break;
            case AttackType.Jump:
                if (jumpHitbox != null) jumpHitbox.col.enabled = true;
                break;
            case AttackType.Down:
                if (downHitbox != null) downHitbox.col.enabled = true;
                break;
            case AttackType.Special:
                //if (specialHitbox != null) specialHitbox.enabled = true;
                break;
        }
    }

    void DisableAllHitboxes()
    {
        if (normalHitbox != null) normalHitbox.col.enabled = false;
        if (upHitbox != null) upHitbox.col.enabled = false;
        if (downHitbox != null) downHitbox.col.enabled = false;
        if (jumpHitbox != null) jumpHitbox.col.enabled = false;
        //if (specialHitbox != null) specialHitbox.enabled = false;
    }
}

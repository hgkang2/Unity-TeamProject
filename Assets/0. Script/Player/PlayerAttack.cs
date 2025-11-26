using System.Globalization;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    PlayerStats stats;

    Animator animator;

    //Player 아래에 각 공격 범위 별로 히트박스 만들기 후 
    //4개다 Player-PlayerAttack 인스펙터에 끝어다 넣기(Player 하위에 만들어논거 참고)
    [Header("각 공격별 히트박스")]
    [SerializeField] PlayerHitBox normalHitbox;
    [SerializeField] PlayerHitBox upHitbox;
    [SerializeField] PlayerHitBox downHitbox;
    //[SerializeField] PlayerHitBox specialHitbox;
    [SerializeField] PlayerHitBox jumpHitbox;

    public bool isAttacking;
    AttackType currentType = AttackType.None;


    void Awake()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
    }
    void Start()
    {
        DisableAllHitboxes();
    }
    
    void OnEnable()
    {
        normalHitbox.OnHit += HandleHit;
        upHitbox.OnHit += HandleHit;
        downHitbox.OnHit += HandleHit;
        //specialHitbox.OnHit += HandleHit;
        jumpHitbox.OnHit += HandleHit;
    }
    void OnDisable()
    {
        normalHitbox.OnHit -= HandleHit;
        upHitbox.OnHit -= HandleHit;
        downHitbox.OnHit -= HandleHit;
        //specialHitbox.OnHit -= HandleHit;
        jumpHitbox.OnHit -= HandleHit;
    }

    void HandleHit(IDamageable target)
    {
        Debug.Log($"{stats.curDamage}");
        target.TakeDamage(stats.curDamage);
    }

    void Update()
    {
        // 키는 방향키+A 등 조합으로 바꾸기
        if (Input.GetKeyDown(KeyCode.A) && !isAttacking)
        {
           //윗 방향키 + A
           if (Input.GetKey(KeyCode.UpArrow))
            {
                StartAttack(AttackType.Up);
            }
            //아래 방향키 + A
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                StartAttack(AttackType.Down);
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                StartAttack(AttackType.Jump);
            }
            else
            {
                StartAttack(AttackType.Normal);
            }
        }
        if (Input.GetKeyDown(KeyCode.S) && !isAttacking)
        {
            StartAttack(AttackType.Special);
        }
    }

    //Animator에 각 Trigger Parameter 추가하기(오타주의)
    public void StartAttack(AttackType type)
    {
        currentType = type;
        isAttacking = true;

        switch (type)
        {
            case AttackType.Normal: // 1
            Debug.Log($"normalAttack");
                animator.SetTrigger("Attack_Normal");
                break;
            case AttackType.Up: // 2
            Debug.Log($"UpAttack");
                animator.SetTrigger("Attack_Up");
                break;
            case AttackType.Down: // 3
            Debug.Log($"DownAttack");
                animator.SetTrigger("Attack_Down");
                break;
            case AttackType.Special: // 4
                animator.SetTrigger("Attack_Special");
                break;
            case AttackType.Jump: // 5
            Debug.Log($"jumpAttack");
                animator.SetTrigger("Attack_Jump");
                break;
        }
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
    public void OnHitboxOff(int attackId)
    {
        DisableAllHitboxes();
    }

    // 각 공격 애니메이션의 마지막 프레임에 연결
    public void OnAttackEnd(int attackId)
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
            case AttackType.Down:
                if (downHitbox != null) downHitbox.col.enabled = true;
                break;
            case AttackType.Jump:
                if(jumpHitbox != null) jumpHitbox.col.enabled = true;
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
        //if (specialHitbox != null) specialHitbox.enabled = false;
        if (jumpHitbox != null) jumpHitbox.col.enabled = false;
    }
}

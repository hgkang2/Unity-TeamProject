using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    HP hp;
    public HP HP => hp;
    Exp exp;
    public Exp Exp => exp;
    PlayerStats stats;
    public PlayerStats Stats => stats;
    Rigidbody2D rb; // 변수 선언은 소문자로 시작. 단 rigidbody2D같은 일부 예약어는 사용 불가해서 rb로 바꿈
    Collider2D collider2d;
    Animator anim;
    SpriteRenderer spriter;
    public Vector2 inputVec;
    public bool isGrounded = true;
    public bool isAttcking = false;
    private float dodgeEndTime;
    private float cooldownEndTime;
    private float dodgeTime = 0.3f;
    private float dodgeCooldown = 1f;
    private bool isDodging = false;
    private float originalGravityScale;
    public float apexGravityScale = 0.1f;
    public float apexThreshold = 0.7f;

    private void Awake()
    {
        hp = GetComponent<HP>();
        exp = GetComponent<Exp>();
        stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        collider2d = GetComponent<Collider2D>();
        originalGravityScale = rb.gravityScale;
    }
    void OnAbled(){
        hp.OnDied += HandleDie;
    }
    void OnDisabled(){
        hp.OnDied -= HandleDie;
    }
    void Update()
    {
        if (HP.IsDead)
        return;
        Move();
        if (isDodging)
        {  
            if (Time.time >= dodgeEndTime)
            {
                EndDodge();
            }
            return; 
        }

        HandleDodgeInput();
    }
    void HandleDodgeInput()
    {
    
        if (Input.GetKeyDown(KeyCode.D) && Time.time >= cooldownEndTime)
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
        float dodgeDirection = spriter.flipX ? 1f : -1f;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(dodgeDirection * stats.MoveSpeed * 2f, 0);
    }

    void EndDodge()
    {
        isDodging = false;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
    }

    void HandleDie(){
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; 
            rb.isKinematic = true; 
        }

        Destroy(gameObject, 2f); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject go = collision.gameObject;
        //수평 벽은 Ground, 수직 벽은 Wall로 일단 했음
        //일단은 벽타기도 가능하게 함
        if ((go.CompareTag("Ground") || go.CompareTag("Wall")) && !isGrounded)
        {
            isGrounded = true;
            anim.SetBool ("IsJumping",false);
            anim.SetTrigger("Land");
            rb.gravityScale = originalGravityScale;

        }
    }
    public void TakeDamage(float amount)
    {
        hp.TakeDamage(amount);
    }

    void Move()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isDodging)
        {
            rb.AddForce(Vector2.up * stats.curJumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            anim.SetTrigger("Jump");
            anim.SetBool("IsJumping", true);
        }

        float horizontalMovement = 0f;

        if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalMovement = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalMovement = -1f;
        }
        inputVec = new Vector2(horizontalMovement, Input.GetAxisRaw("Vertical"));
        rb.linearVelocity = new Vector2(horizontalMovement * stats.curMoveSpeed, rb.linearVelocity.y);
        if (!isDodging)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * stats.curMoveSpeed, rb.linearVelocity.y);
        }
    }

    void LateUpdate()
    {
        if(PauseManager.IsPaused) return;
        
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        anim.SetFloat("Move", Mathf.Abs(rb.linearVelocity.x));
        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    void FixedUpdate() 
    { 
        if (HP.IsDead || isDodging) return;
        if(!isGrounded)
        {
            if (Mathf.Abs(rb.linearVelocity.y) < apexThreshold && rb.linearVelocity.y >= 0)
            {
                rb.gravityScale = apexGravityScale;
            }
            else if (rb.linearVelocity.y < 0 )
            {
                rb.gravityScale = originalGravityScale;
            }
            else
            {
                rb.gravityScale = originalGravityScale;
            }

        }
    }
    

}
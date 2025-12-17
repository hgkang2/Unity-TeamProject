using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Nightfang : MonsterBase
{
    int facingX;
    Vector3 originScale;

    public override void Awake()
    {
        base.Awake();
        originScale = transform.localScale;
        facingX = 1;
    }

    public override void Update()
    {
        base.Update();

        if (currentState == MonsterStateType.Patrol)
        {
            facingX = patrolDirX;
        }
        else if (currentState == MonsterStateType.Aggro && detector != null)
        {
            if (isAttack || isUsingSkill) return;

            float deadZone = 0.05f;

            if (Mathf.Abs(detector.dx) > deadZone)
                facingX = detector.moveDirx;
        }

        // flip은 항상 facingX 기준
        transform.localScale = new Vector3(
            facingX < 0 ? originScale.x : -originScale.x,
            originScale.y,
            originScale.z
        );
    }

    public override void Attack()
    {
        SkillCol.SetActive(true);
        Vector2 dashDir = Vector2.right * facingX;
        rb.AddForce(2f * dashDir, ForceMode2D.Impulse);
    }

    public override void OnAttackExit()
    {
        SkillCol.SetActive(false);
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        mover.StopX();

        yield return new WaitForSeconds(monsterStats.attackRate);

        animator.SetTrigger("Aggro");
        mover.MoveX(detector != null ? detector.moveDirx : patrolDirX, monsterStats.aggroSpeed);
        isAttack = false;
        isAttackReady = true;
    }

    public override void UseSkill()
    {
        Vector2 dashDir = Vector2.right * facingX;
        rb.AddForce(10f * dashDir, ForceMode2D.Impulse);
    }

    public override void OnSkillExit()
    {
        StartCoroutine(SkillCooldown());
    }

    IEnumerator SkillCooldown()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        yield return new WaitForSeconds(monsterStats.skillDelay);
        
        isUsingSkill = false;
        animator.SetTrigger("Aggro");
        ChangeState(MonsterStateType.Aggro);

        yield return new WaitForSeconds(monsterStats.skillCoolTime);

        isSkillReady = true;
    }
}
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Grimlog : MonsterBase
{   
    Vector3 originScale;
    public override void Awake()
    {
        base.Awake();
        direction.x = 1;
        originScale = transform.localScale;
    }

    public override void Update()
    {
        base.Update();

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(-1*originScale.x,originScale.y,originScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = originScale;
        }
    }

    public override void UseSkill()
    {
        rb.AddForce(10f * direction, ForceMode2D.Impulse);
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

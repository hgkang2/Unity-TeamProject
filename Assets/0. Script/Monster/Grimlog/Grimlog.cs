using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Grimlog : MonsterBase
{
    [SerializeField]
    GameObject HitBox;
    [SerializeField]
    GameObject DetectBox;

    public override void Awake()
    {
        base.Awake();
        direction.x = 1;
    }

    public override void Update()
    {
        base.Update();

        if (direction.x > 0)
        {
            spriteRenderer.flipX = true;
            DetectBox.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
            DetectBox.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public override void MonsterDataSetting()
    {
        monsterData.IdleTime = 3f;
        monsterData.PatrolSpeed = 3f;
        monsterData.PatrolTime = 3f;

        monsterData.MoveDirection = -1;

        monsterData.AggroRange = 13f;

        monsterData.Skill_Damage = 1f;
        monsterData.Skill_Delay = 3f;

        monsterData.SkillA_ActiveRange = 6f;
        monsterData.SkillA_coolTime = 12f;
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

        yield return new WaitForSeconds(monsterData.Skill_Delay);
        
        isUsingSkill = false;
        animator.SetTrigger("Aggro");
        ChangeState(MonsterStateType.Aggro);

        yield return new WaitForSeconds(monsterData.SkillA_coolTime);

        isSkillReady = true;
    }
}

using System.Collections;
using UnityEngine;

public class Felmos : MonsterBase
{
    int patrolDirIndex = 0;
    Vector2[] patrolDir = new Vector2[]
    { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

    float retreatRange = 4f;
    [SerializeField] float MinHeight = 3f;
    [SerializeField] bool retreating;

    [SerializeField] GameObject FelmosBullet;

    [SerializeField] GroundHeightLimiter heightLimiter;
    [SerializeField] KeepDistance keepDistance;

    LayerMask Ground = 7;

    public Transform PlayerPos;
    public Transform FirePos;

    public override void Awake()
    {
        base.Awake();

        //if (heightLimiter = null) heightLimiter = GetComponent<GroundHeightLimiter>();
        //if (keepDistance = null) keepDistance = GetComponent<KeepDistance>();

        //PlayerPos = GameObject.Find("Player").transform;
        //keepDistance.SetTarget(GameObject.Find("Player").transform);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Patrol()
    {
        Vector2 dir = patrolDir[patrolDirIndex];    

        rb.linearVelocity = dir * monsterStats.patrolSpeed;

        if (StateTimer >= monsterStats.patrolTime)
        {
            patrolDirIndex = (patrolDirIndex + 1) % patrolDir.Length;

            animator.SetTrigger("Idle");
            ChangeState(MonsterStateType.Idle);
        }

        //if(DistanceToPlayer <= monsterStats.aggroRange)
        //{
        //    animator.SetTrigger("Alert");
        //    ChangeState(MonsterStateType.Aggro);
        //}
    }

    public override void Aggro()
    {
        if (isUsingSkill || retreating) return;

        rb.linearVelocity = detector.dirToPlayer * monsterStats.aggroSpeed;

        //keepDistance?.TryRetreat();

        //if (DistanceToPlayer >= monsterStats.aggroRange * 1.2f)
        //{
        //    animator.SetTrigger("Idle");
        //    ChangeState(MonsterStateType.Idle);
        //}

        //if (DistanceToPlayer <= monsterStats.skillActiveRange && isSkillReady && !isUsingSkill)
        //{
        //    animator.SetTrigger("ReadySkill");
        //    isUsingSkill = true;
        //    isSkillReady = false;
        //}
    }

    public override void UseSkill()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public override void OnSkillExit()
    {
        var ShootSkill = Instantiate(FelmosBullet, FirePos.position, Quaternion.identity);

        Vector2 dir = PlayerPos.position - FirePos.position;
        float damage = monsterStats.skillDamage;

        ShootSkill.GetComponent<FelmosBullet>().Initialize(dir, damage);

        StartCoroutine(SkillCooldown());
    }

    IEnumerator SkillCooldown()
    {
        yield return new WaitForSeconds(monsterStats.skillDelay);

        isUsingSkill = false;
        animator.SetTrigger("Aggro");
        ChangeState(MonsterStateType.Aggro);

        yield return new WaitForSeconds(monsterStats.skillCoolTime);

        isSkillReady = true;
    }
}
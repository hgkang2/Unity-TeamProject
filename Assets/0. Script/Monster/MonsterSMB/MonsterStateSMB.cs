using UnityEngine;

public abstract class MonsterStateSMB : StateMachineBehaviour
{
    protected MonsterBase monsterBase;

    private bool initialized = false;

    void Init(Animator animator)
    {
        if (initialized) return;
        monsterBase = animator.GetComponentInParent<MonsterBase>();
        initialized = true;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Init(animator);
        OnEnter();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Init(animator);
        OnUpdate();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Init(animator);
        OnExit();
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}

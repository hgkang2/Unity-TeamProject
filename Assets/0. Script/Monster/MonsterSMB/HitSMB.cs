using UnityEngine;

public class HitSMB : StateMachineBehaviour
{
    Nightfang nightfang;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null)
            nightfang = animator.GetComponentInParent<Nightfang>();
        nightfang.isHit = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null) return;
        nightfang.isHit = false;
    }
}

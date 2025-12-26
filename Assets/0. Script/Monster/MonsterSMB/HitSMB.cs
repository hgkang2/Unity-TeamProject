using UnityEngine;

public class HitSMB : StateMachineBehaviour
{
    NightfangStandalone nightfang;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null)
            nightfang = animator.GetComponentInParent<NightfangStandalone>();
        nightfang.isHit = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null) return;
        nightfang.isHit = false;
    }
}

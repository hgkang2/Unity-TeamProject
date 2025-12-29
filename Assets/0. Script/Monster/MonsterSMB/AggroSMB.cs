using UnityEngine;

public class AggroSMB : StateMachineBehaviour
{
    NightfangStandalone nightfang;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (nightfang == null)
            nightfang = animator.GetComponentInParent<NightfangStandalone>();
            
    }
}

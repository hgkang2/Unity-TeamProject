using UnityEngine;

public class AttackSMB : StateMachineBehaviour
{
    NightfangStandalone nightfang;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null)
            nightfang = animator.GetComponentInParent<NightfangStandalone>();

        nightfang.attackHitboxObj.SetActive(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null) return;
        nightfang.attackHitboxObj.SetActive(false);
    }
}

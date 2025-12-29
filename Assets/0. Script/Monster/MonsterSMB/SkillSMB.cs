using UnityEngine;

public class SkillSMB : StateMachineBehaviour
{
    NightfangStandalone nightfang;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null)
            nightfang = animator.GetComponentInParent<NightfangStandalone>();
        nightfang.spriteRenderer.color = Color.red;
        nightfang.skillHitBoxObj.SetActive(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null) return;
        nightfang.spriteRenderer.color = Color.white;
        nightfang.skillHitBoxObj.SetActive(false);
    }
}

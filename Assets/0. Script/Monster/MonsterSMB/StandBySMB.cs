using UnityEngine;

public class StandBySMB : StateMachineBehaviour
{
    NightfangStandalone nightfang;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // StandBy 상태에 들어올 때 한 번만 찾아서 캐싱
        if (nightfang == null)
            nightfang = animator.GetComponentInParent<NightfangStandalone>();
        // 또는 animator.GetComponent<NightfangStandalone>();
        // (NightfangStandalone이 Animator 오브젝트에 붙어있으면 GetComponent, 부모면 InParent)
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null) return; // 안전장치
        nightfang.ChangeState(NightfangStandalone.State.Aggro);
    }
}

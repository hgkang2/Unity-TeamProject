using UnityEngine;

public class StandBySMB : StateMachineBehaviour
{
    Nightfang nightfang;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // StandBy ���¿� ���� �� �� ���� ã�Ƽ� ĳ��
        if (nightfang == null)
            nightfang = animator.GetComponentInParent<Nightfang>();
        // �Ǵ� animator.GetComponent<NightfangStandalone>();
        // (NightfangStandalone�� Animator ������Ʈ�� �پ������� GetComponent, �θ�� InParent)
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (nightfang == null) return; // ������ġ
        nightfang.ChangeState(Nightfang.State.Aggro);
    }
}

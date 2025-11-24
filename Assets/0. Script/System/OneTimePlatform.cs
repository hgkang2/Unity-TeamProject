using UnityEngine;
using DG.Tweening;

public class OneTimePlatform : MonoBehaviour
{
    public float duration = 2;
    public float strength = 0.2f;
    public int vivtaro = 5;
    public float randoness = 10;

    bool triggered = false;
    void OnCollisionEnter2D(Collision2D other)
    {

        if (other.gameObject.GetComponent<Player>())
        {
            if(triggered) return;
            triggered = true;

            Sequence seq = DOTween.Sequence(); // 시퀀스 생성
            seq.Append(transform.DOShakePosition(duration, strength, vivtaro, randoness, false, true, ShakeRandomnessMode.Full))
            .Append(transform.DOLocalMoveY(-20, 2)).SetEase(Ease.InQuad)
            .OnComplete(() => Destroy(gameObject));
        }
    }
}

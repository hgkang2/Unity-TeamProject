using DG.Tweening;
using UnityEngine;

public class RerollButton : MonoBehaviour
{
    Sequence s;
    public void Click()
    {
        if (s != null) s.Kill();
        s = DOTween.Sequence();
        s.Append(transform.DOScale(0.95f, 0.25f).SetEase(Ease.OutQuint))
        .Append(transform.DOScale(1f, 0.25f).SetEase(Ease.InQuart))
        .SetUpdate(true);
    }
}

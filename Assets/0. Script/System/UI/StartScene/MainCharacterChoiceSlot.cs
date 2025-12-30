using System;
using DG.Tweening;
using UnityEngine;

public class MainCharacterChoiceSlot : MonoBehaviour
{
    RectTransform rect;

    public event Action<CharacterId> characterSelected;
    public void RaiseCharacterSelect(int num) => characterSelected?.Invoke((CharacterId)num);

    public float hoverScale = 1.05f;
    public float tweenDuration = 0.15f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Selected()
    {
        rect.DOKill(); // 기존 트윈 제거
        rect.DOScale(hoverScale, tweenDuration)
            .SetEase(Ease.OutBack);
    }

    public void DeSelected()
    {
        rect.DOKill();
        rect.DOScale(0.95f, tweenDuration)
            .SetEase(Ease.OutQuad);
    }
}

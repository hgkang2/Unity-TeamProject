using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainCharacterChoiceSlot : MonoBehaviour
{
    CanvasGroup cg;
    [SerializeField] public CharacterId id;
    [SerializeField] Image backgroundImage;
    [SerializeField] Image deSelectedImage;
    [SerializeField] Image selectedImage;
    [SerializeField] Image lockedImage;
    RectTransform rect;

    public event Action<CharacterId> slotselected;
    public void RaiseSlotSelected() => slotselected?.Invoke(id);
    public event Action<CharacterId> slotFocused;
    public void RaiseSlotFocused() => slotFocused?.Invoke(id);
    public event Action<CharacterId> slotUnFocused;
    public void RaiseSlotUnFocused() => slotUnFocused?.Invoke(id);

    public float hoverScale = 1.05f;
    public float tweenDuration = 0.15f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void SetSlot(bool isLockedCharacter)
    {
        if (isLockedCharacter)
        {
            lockedImage.enabled = false;
        }
        else
        {
            lockedImage.enabled = true;
        }
        deSelectedImage.enabled = true;
        selectedImage.enabled = false;
    }

    public void Focus()
    {
        deSelectedImage.enabled = false;
        selectedImage.enabled = true;
        rect.DOKill(); // 기존 트윈 제거
        rect.DOScale(hoverScale, tweenDuration)
            .SetEase(Ease.OutBack);
    }

    public void UnFocus()
    {
        deSelectedImage.enabled = true;
        selectedImage.enabled = false;
        rect.DOKill();
        rect.DOScale(0.95f, tweenDuration)
            .SetEase(Ease.OutQuad);
    }
}

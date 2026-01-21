using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 포커스 상태에 따라 이미지를 전환하는 뷰 컴포넌트
/// - 부모 패널이 SetFocused()로 제어
/// </summary>
public class FocusImageSwitcher : MonoBehaviour, IFocusableView
{
    [Header("Target Image")]
    [SerializeField] Image targetImage;

    [Header("Sprites")]
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite focusedSprite;

    bool isFocused;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        Apply();
    }

    public void SetFocused(bool focused)
    {
        if (isFocused == focused)
            return;

        isFocused = focused;
        Apply();
    }

    void Apply()
    {
        if (targetImage == null)
            return;

        targetImage.sprite = isFocused ? focusedSprite : normalSprite;
    }
}

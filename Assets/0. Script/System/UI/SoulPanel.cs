using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.Android.Gradle.Manifest;

public class SoulPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("이동 설정")]
    public float hoverScale = 1.00f;      // 얼마나 커질지
    public float hoverDuration = 0.2f;  // 커지는 시간
    public float backDuration = 0.15f;  // 원복 시간
    public Ease hoverEase = Ease.OutQuad;
    public Ease backEase = Ease.InQuad;

    [SerializeField] CanvasGroup contentGroup;
    [SerializeField] Image soulImage;
    [SerializeField] TMP_Text soulName;
    [SerializeField] TMP_Text soulEffect;
    [SerializeField] TMP_Text soulDescript;
    SoulData soulData;
    public SoulData SoulData => soulData;

    public event Action<SoulPanel> SoulMouseEntered;
    public event Action<SoulPanel> SoulMouseExited;
    public event Action<SoulPanel> SoulMouseClicked;

    RectTransform rect;
    Vector2 originalScale;
    Tween moveTween;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    void OnEnable()
    {
        KillTween();
        if (rect != null)
        {
            rect.localScale = originalScale;
        }
        soulData = null;
    }
    void KillTween()
    {
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
            moveTween = null;
        }
    }


    public void Set(SoulData data)
    {
        soulImage.sprite = data.soulSprite;
        soulName.text = data.displayName;
        soulEffect.text = data.soulEffectText;
        soulDescript.text = data.soulDescript;
        soulData = data;
        OriginPanelScale();
        Debug.Log($"{data.displayName}");
    }

    public void visibleContent()
    {
        contentGroup.alpha = 1f;
    }
    public void InvisibleContent()
    {
        contentGroup.alpha = 0f;
    }

    public void ExpandPanelScale()
    {
        KillTween();
        if (hoverScale == 1) return;

        moveTween = rect.DOScale(hoverScale, hoverDuration)
            .SetEase(hoverEase)
            .SetUpdate(true);
    }
    public void OriginPanelScale()
    {
        KillTween();
        moveTween = rect.DOScale(1, backDuration)
        .SetEase(backEase)
            .SetUpdate(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoulMouseEntered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SoulMouseExited?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if(soulData == null) Debug.Log($"클릭한 패널의 soulData 없음");
        SoulMouseClicked?.Invoke(this);
    }
}
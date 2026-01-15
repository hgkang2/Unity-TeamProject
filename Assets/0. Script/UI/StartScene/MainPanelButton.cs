using ChocDino.UIFX;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainPanelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    MainPanel mainPanel;
    Button button;
    GlowFilter glowFilter;
    void Awake()
    {
        mainPanel = GetComponentInParent<MainPanel>();
        button = GetComponent<Button>();
        glowFilter = GetComponentInChildren<GlowFilter>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mainPanel.ButtonMouseEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mainPanel.ButtonMouseExit();
    }


    Tween glowTween;

    public void Focused()
    {
        AnimateGlow(0.5f, 0.12f);
    }

    public void UnFocused()
    {
        AnimateGlow(0.2f, 0.18f);
    }

    void AnimateGlow(float target, float duration)
    {
        glowTween?.Kill();
        glowTween = DOTween.To(
            () => glowFilter.Strength,
            v => glowFilter.Strength = v,
            target,
            duration
        );
    }

    public void Confirm()
    {
        button.onClick.Invoke();
    }

}

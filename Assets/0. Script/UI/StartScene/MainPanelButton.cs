using ChocDino.UIFX;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainPanelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    MainPanel mainPanel;
    Button button;
    TMP_Text text;
    GlowFilter glowFilter;
    void Awake()
    {
        mainPanel = GetComponentInParent<MainPanel>();
        button = GetComponent<Button>();
        text = GetComponentInChildren<TMP_Text>();
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

    public void Focused()
    {
        AnimateTextColor(0xffffff);
        AnimateGlow(0.5f, 0.12f);
    }

    public void UnFocused()
    {
        AnimateTextColor(0xe0e0e0);
        AnimateGlow(0.2f, 0.18f);
    }

    Tween colorTween;
    Tween glowTween;
    void AnimateTextColor(int rgb, float duration = 0.12f)
    {
        if (text == null) return;

        Color target = HexRGB(rgb);

        colorTween?.Kill();
        colorTween = DOTween.To(
            () => text.color,
            v => text.color = v,
            target,
            duration
        );
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

    static Color HexRGB(int rgb)
    {
        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);
        return new Color32(r, g, b, 255);
    }

    public void Confirm()
    {
        button.onClick.Invoke();
    }

}

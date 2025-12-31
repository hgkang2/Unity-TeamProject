using ChocDino.UIFX;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class Elevator : MonoBehaviour, IInteractable
{
    [SerializeField] Animator leverAnimator;
    [SerializeField] SpriteRenderer leverSprite;
    [SerializeField] Transform targetPos;


    private void Awake()
    {
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Exit()
    {

    }
    public void Interact(Player player)
    {
        leverAnimator.SetTrigger("Interact");
        leverAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        StartCoroutine(GameManager.Instance.TeleportRoutine(player, targetPos));
    }

    public bool IsAvailable()
    {
        return true;
    }

    public void OnFocus()
    {
        HighlightOn();
        leverAnimator.SetTrigger("OnFocus");
        leverAnimator.updateMode = AnimatorUpdateMode.Normal;
    }

    public void OnUnfocus()
    {
        HighlightOff();
    }

    Tween highlightTween;

    public void HighlightOn()
    {
        highlightTween?.Kill();
        highlightTween = leverSprite.DOColor(
            new Color(1.5f, 1.5f, 1.5f, 1f),
            0.15f
        );
    }

    public void HighlightOff()
    {
        highlightTween?.Kill();
        highlightTween = leverSprite.DOColor(Color.white, 0.15f);
    }

}

using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Elevator : MonoBehaviour, IInteractable
{
    [Header("순간이동할 위치")]
    [SerializeField] Transform targetPos;


    [Header("아래는 초기화용 건들지말기")]
    //[SerializeField] Animator leverAnimator;
    [SerializeField] SpriteRenderer buttonSprite;


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
        //leverAnimator.SetTrigger("Interact");
        //leverAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        StartCoroutine(GameManager.Instance.TeleportRoutine(player, targetPos));
        StartCoroutine(Initialize());
    }
    IEnumerator Initialize()
    {
        yield return new WaitForSeconds(5);
        //leverAnimator.SetTrigger("Initialize");
        //leverAnimator.updateMode = AnimatorUpdateMode.Normal;
        HighlightOff();
    }

    public bool IsAvailable()
    {
        return true;
    }

    public void OnFocus()
    {
        HighlightOn();
        //leverAnimator.SetTrigger("OnFocus");
    }

    public void OnUnfocus()
    {
        HighlightOff();
    }

    Tween highlightTween;

    public void HighlightOn()
    {
        highlightTween?.Kill();
        highlightTween = buttonSprite.DOColor(
            new Color(1.8f, 1.8f, 1.8f, 1f),
            0.15f
        );
    }

    public void HighlightOff()
    {
        highlightTween?.Kill();
        highlightTween = buttonSprite.DOColor(Color.white, 0.15f);
    }

}

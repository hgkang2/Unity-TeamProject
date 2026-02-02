using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Portal : MonoBehaviour, IInteractable
{
    [Header("순간이동할 위치")]
    [SerializeField] Transform targetPos;
    [Header("순간이동할 맵의 배경")]
    [SerializeField] BGSetKey BGkey;


    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Transform interactionUIPos;
    public Transform InteractionUIPosition => interactionUIPos;

    LocalSFX localSFX;

    void Awake()
    {
        localSFX = GetComponent<LocalSFX>();
    }
    void Start()
    {
        localSFX.Play("Open");
        localSFX.PlayLoop("Continue");
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Exit()
    {

    }
    public void Interact(Player player, Interactor interactor)
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        StartCoroutine(GameManager.Instance.TeleportRoutine(player, targetPos, BGkey));
        StartCoroutine(Close());
        localSFX.Play("Horolrolro");
    }

    IEnumerator Close()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        localSFX.Play("Close");
        animator.SetTrigger("Close");
    }

    public bool IsAvailable()
    {
        return true;
    }

    public void OnFocus()
    {
        HighlightOn();
    }

    public void OnUnfocus()
    {
        HighlightOff();
    }

    Tween highlightTween;

    public void HighlightOn()
    {
        highlightTween?.Kill();
        highlightTween = sprite.DOColor(
            new Color(1.5f, 1.5f, 1.5f, 1f),
            0.15f
        );
    }

    public void HighlightOff()
    {
        highlightTween?.Kill();
        highlightTween = sprite.DOColor(Color.white, 0.15f);
    }
}

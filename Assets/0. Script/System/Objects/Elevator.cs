using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class Elevator : MonoBehaviour, IInteractable
{
    [SerializeField] Animator leverAnimator;
    [SerializeField] Transform leverPos;
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

    public Vector2 GetInteractPoint()
    {
        return leverPos.position;
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
        leverAnimator.SetTrigger("OnPocus");
        leverAnimator.updateMode = AnimatorUpdateMode.Normal;
    }

    public void OnUnfocus()
    {
        
    }

}

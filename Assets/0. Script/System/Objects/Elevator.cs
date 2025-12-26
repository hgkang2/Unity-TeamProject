using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class Elevator : IInteractable
{
    [SerializeField] Animator leverAnimator;

    private void Awake()
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CanInteract()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetInteractPoint()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(Player user)
    {
        leverAnimator.SetTrigger("Interact");
    }

    public bool IsAvailable()
    {
        throw new System.NotImplementedException();
    }

    public void OnFocus()
    {
        
    }

    public void OnUnfocus()
    {
        throw new System.NotImplementedException();
    }

}

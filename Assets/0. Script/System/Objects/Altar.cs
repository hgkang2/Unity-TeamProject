using UnityEngine;

public class Altar : MonoBehaviour, IInteractable
{
    [SerializeField] Transform interactionBound;

    [SerializeField] GameObject candle1;
    [SerializeField] GameObject candle2;
    [SerializeField] GameObject candle3;

    int offeredFlame = 0;
    bool isActivated = false;

    void Awake()
    {
        candle1.SetActive(false);
        candle2.SetActive(false);
        candle3.SetActive(false);
    }


    public Transform InteractionUIPosition => interactionBound;

    public bool CanInteract()
    {
        return true;
    }

    public void Exit()
    {
        return;
    }

    public void Interact(Player user, Interactor interactor)
    {
        switch (GameManager.Instance.hasFlame)
        {
            case 0:

            break;
            case 1:

            break;
            case 2:

            break;
            case 3:

            break;
        }
    }

    public bool IsAvailable()
    {
        return true;
    }

    public void OnFocus()
    {
        return;
    }

    public void OnUnfocus()
    {
        return;
    }
}

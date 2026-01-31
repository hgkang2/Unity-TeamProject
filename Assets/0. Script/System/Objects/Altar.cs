using UnityEngine;

public class Altar : MonoBehaviour, IInteractable
{
    AltarUI altarUI;
    [SerializeField] Transform interactUIPos;

    [SerializeField] GameObject[] candles;

    int offeredFlame = 0;
    bool isActivated = false;

    void Awake()
    {
        SceneContext sc = FindFirstObjectByType<SceneContext>();
        altarUI = sc.altarUI;
        
        foreach(var candle in candles) candle.SetActive(false);
    }

    void OnEnable()
    {
        GameManager.Instance.changedUsedFlame += SetFlameUsedImage;
    }

    void OnDisable()
    {
        GameManager.Instance.changedUsedFlame -= SetFlameUsedImage;
    }

    void SetFlameUsedImage()
    {
        int count = GameManager.Instance.UsedFlame;

        for (int i = 0; i < candles.Length; i++)
        {
            candles[i].SetActive(i < count);
        }

        if(count == 3) isActivated = true;
    }


    public Transform InteractionUIPosition => interactUIPos;

    public bool CanInteract()
    {
        return !isActivated;
    }

    public void Exit()
    {
        return;
    }

    public void Interact(Player user, Interactor interactor)
    {
        altarUI.Open();
        altarUI.interactor = interactor;
    }

    public bool IsAvailable()
    {
        return !isActivated;
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

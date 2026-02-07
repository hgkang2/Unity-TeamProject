using System.Collections;
using UnityEngine;

public class Altar : MonoBehaviour, IInteractable
{
    AltarUI altarUI;
    [SerializeField] Transform interactUIPos;
    [SerializeField] GameObject portal;

    [SerializeField] GameObject[] candles;

    [SerializeField] SystemEventChannel systemEventChannel;

    int offeredFlame = 0;
    bool isActivated = false;

    void Awake()
    {
        SceneContext sc = FindFirstObjectByType<SceneContext>();
        altarUI = sc.altarUI;

        foreach (var candle in candles) candle.SetActive(false);
    }

    GameManager GM;
    Coroutine bindCo;
    bool bound;

    void OnEnable()
    {
        if (bound) return;

        if (bindCo == null) bindCo = StartCoroutine(Co_BindGM());
    }

    void OnDisable()
    {
        if (bindCo != null)
        {
            StopCoroutine(bindCo);
            bindCo = null;
        }
        
        if (bound && GM != null)
        {
            GM.changedUsedFlame -= SetFlameUsedImage;
            GM.AltarActivated -= AltarActivate;
            bound = false;
        }

        GM = null;
    }

    IEnumerator Co_BindGM()
    {
        // GM이 생길 때까지 대기
        while (GameManager.Instance == null)
            yield return null;

        GM = GameManager.Instance;

        // 중복 구독 방지
        if (!bound)
        {
            GM.changedUsedFlame += SetFlameUsedImage;
            GM.AltarActivated += AltarActivate;
            bound = true;
        }

        bindCo = null;
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
    void AltarActivate()
    {
        isActivated = true;
        portal.SetActive(true);
        systemEventChannel.RaiseAltarActivate();
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

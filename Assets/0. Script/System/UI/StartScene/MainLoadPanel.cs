using UnityEngine;

public class MainLoadPanel : UIKeyboardHandler
{

    CanvasGroup cg;
    LoadPanelEventAggregator aggregator;

    [SerializeField] MainLoadSlot[] slots;
    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        aggregator = GetComponent<LoadPanelEventAggregator>();
    }

    void Start()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
    }

    void OnEnable()
    {
        if (aggregator != null)
        {
            aggregator.MouseEntered += HandleSlotEnter;
            aggregator.MouseExited += HandleSlotExit;
            aggregator.LeftClicked += HandleSlotLeftClick;
        }
    }

    void OnDisable()
    {
        if (aggregator != null)
        {
            aggregator.MouseEntered -= HandleSlotEnter;
            aggregator.MouseExited -= HandleSlotExit;
            aggregator.LeftClicked -= HandleSlotLeftClick;
        }
    }
    void HandleSlotEnter(SlotEventArgs<SaveData> args)
    {
        // 여기서 패널 단위 로직 처리 (프리뷰 패널 갱신 등)
    }

    void HandleSlotExit(SlotEventArgs<SaveData> args)
    {
    }

    void HandleSlotLeftClick(SlotEventArgs<SaveData> args)
    {
    }

    public void Open()
    {
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        enabled = true;
    }

    public void Close()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        enabled = false;
    }

    protected override void OnUIMove(Vector2 dir)
    {

    }
}

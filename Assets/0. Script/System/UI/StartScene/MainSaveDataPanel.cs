using Unity.VisualScripting;
using UnityEngine;

public class MainSaveDataPanel : UIKeyboardHandler
{

    [HideInInspector] public CanvasGroup cg;

    [SerializeField] MainCharacterChoicePanel mainCharacterChoicePanel;
    SaveDataEventAggregator aggregator;

    [SerializeField] MainSaveDataSlot[] slots;
    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        aggregator = GetComponent<SaveDataEventAggregator>();
    }

    void Start()
    {
        aggregator.RebuildViews();
        foreach (var slot in slots)
        {
            slot.Bind(null);
        }

    }

    protected override void OnUIEnabled()
    {
        if (aggregator != null)
        {
            aggregator.MouseEntered += HandleSlotEnter;
            aggregator.MouseExited += HandleSlotExit;
            aggregator.LeftClicked += HandleSlotLeftClick;
        }
    }

    protected override void OnUIDisabled()
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

    }

    void HandleSlotExit(SlotEventArgs<SaveData> args)
    {
    }

    void HandleSlotLeftClick(SlotEventArgs<SaveData> args)
    {
        if (args.Data == null)
        {
            mainCharacterChoicePanel.Open();
        }
        else
        {
            Debug.Log($"저장 불러오기");
        }
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
    protected override void OnUICancel()
    {
        if (mainCharacterChoicePanel.cg.blocksRaycasts)
        {
            return;
        }
        Close();
    }   

    //X 버튼 눌렀을 때
    public void ButtonCancel()
    {
        mainCharacterChoicePanel.Close();
        Close();
    }
}

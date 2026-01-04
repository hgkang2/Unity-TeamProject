using Unity.VisualScripting;
using UnityEngine;

public class MainCharacterChoicePanel : UIKeyboardHandler
{
    [HideInInspector] public CanvasGroup cg;
    [SerializeField] MainCharacterChoiceSlot[] slots;
    [SerializeField] MainCharacterConfirmPanel[] confirmPanels;
    public bool CanClose => !confirmPanels[0].cg.blocksRaycasts
                            && !confirmPanels[1].cg.blocksRaycasts
                            && !confirmPanels[2].cg.blocksRaycasts;

    public int? focusedIndex = -1;
    public int? selectedIndex = -1;
    
    void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        foreach(var slot in slots)
        {
            slot.slotselected += SelectSlot;
            slot.slotFocused += FocusSlot;
            slot.slotUnFocused += UnFocusSlot;
            slot.UnFocus();
        }
    }
    private void Start()
    {
        foreach(var panel in confirmPanels)
        {
            panel.gameObject.SetActive(true);
            panel.Close();
        }
    }
    void FocusSlot(CharacterId id)
    {
        focusedIndex = (int)id-1;
        slots[(int)id-1].Focus();
    }

    void UnFocusSlot(CharacterId id)
    {
        focusedIndex = null;
        slots[(int)id-1].UnFocus();
    }

    void SelectSlot(CharacterId id)
    {
        OnUIConfirm();
    }


    // ConfirmPanel->ConfirmButton OnClick Event
    public void GameStart()
    {
        Debug.Log((selectedIndex+1));
        GameManager.Instance.curcharacter = (CharacterId)(selectedIndex+1);
        SceneLoader.NoLoadingScene("IngameIntro");
    }

    protected override void OnUIMove(Vector2 dir)
    {
        // 현재 아무것도 선택되지 않은 상태라면
        if (focusedIndex == null)
        {
            // 왼쪽 → 0번 선택
            if (dir.x < -0.1f) focusedIndex = 0;
            // 오른쪽 → 마지막 선택
            else if (dir.x > 0.1f) focusedIndex = slots.Length - 1;

            UpdatFocusHighlight();
            return;
        }

        //왼쪽 방향키시 위쪽 방향으로
        if (dir.x < -0.1f) focusedIndex--;
        //오른쪽 방향키시 아래쪽 방향으로
        else if (dir.x > 0.1f) focusedIndex++;

        //min, max 처리
        if (focusedIndex < 0) focusedIndex = 0;
        else if (focusedIndex >= slots.Length) focusedIndex = slots.Length - 1;

        //강조된 버튼 변경
        UpdatFocusHighlight();
    }
    void UpdatFocusHighlight()
    {
        foreach(var slot in slots) slot.UnFocus();
        if(focusedIndex != null)
        {
            slots[(int)focusedIndex].Focus();
        }
    }
    protected override void OnUIConfirm()
    {
        if(focusedIndex == null) return;
        if(slots[(int)focusedIndex].isLocked) return;
        selectedIndex = focusedIndex;
        focusedIndex = null;
        UpdatFocusHighlight();
        
        confirmPanels[(int)selectedIndex].Open();                  

        this.enabled = false;
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
        focusedIndex = null;
        selectedIndex = null;
        UpdatFocusHighlight();

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        enabled = false;
    }
}

using System;
using UnityEngine;

public class MainCharacterChoicePanel : MonoBehaviour, IUIKeyboardTarget
{
    [HideInInspector] public CanvasGroup cg;
    [SerializeField] MainCharacterChoiceSlot[] slots;
    [SerializeField] MainCharacterConfirmPanel confirmPanel;

    public int? focusedIndex = -1;
    public int? selectedIndex = -1;

    
    public event Action RequestOpenCharacterConfirmPanel;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        foreach (var slot in slots)
        {
            slot.slotselected += SelectSlot;
            slot.slotFocused += FocusSlot;
            slot.slotUnFocused += UnFocusSlot;
            slot.UnFocus();
        }
    }
    private void Start()
    {
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.Close();
    }
    void FocusSlot(CharacterId id)
    {
        focusedIndex = (int)id - 1;
        slots[(int)id - 1].Focus();
    }

    void UnFocusSlot(CharacterId id)
    {
        focusedIndex = null;
        slots[(int)id - 1].UnFocus();
    }

    void SelectSlot(CharacterId id)
    {
        ConfirmSelection();
    }

    void ConfirmSelection()
    {
        if (focusedIndex == null) return;
        if (slots[(int)focusedIndex].isLocked) return;

        selectedIndex = focusedIndex;
        focusedIndex = null;
        UpdatFocusHighlight();

        RequestOpenCharacterConfirmPanel?.Invoke();
    }
    // ConfirmPanel->ConfirmButton OnClick Event
    public void GameStart()
    {
        Debug.Log((selectedIndex + 1));
        GameManager.Instance.curcharacter = (CharacterId)(selectedIndex + 1);
        SceneLoader.NoLoadingScene("IngameIntro");
    }

    void UpdatFocusHighlight()
    {
        foreach (var slot in slots) slot.UnFocus();
        if (focusedIndex != null)
        {
            slots[(int)focusedIndex].Focus();
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
        focusedIndex = null;
        selectedIndex = null;
        UpdatFocusHighlight();

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        enabled = false;
    }

    void IUIKeyboardTarget.OnUIMove(Vector2 dir)
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

    void IUIKeyboardTarget.OnUIConfirm()
    {
        ConfirmSelection();
    }

    public void OnUICancel()
    {
        throw new System.NotImplementedException();
    }
}

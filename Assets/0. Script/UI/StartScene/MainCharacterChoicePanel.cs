using System;
using UnityEngine;

public class MainCharacterChoicePanel : UIPanelBase
{
    [SerializeField] MainCharacterChoiceSlot[] slots;
    [SerializeField] ConfirmPanel confirmPanel;

    public int? focusedIndex = -1;
    public int? selectedIndex = -1;

    protected override void Init()
    {
        foreach (var slot in slots)
        {
            slot.slotselected += SelectSlot;
            slot.slotFocused += FocusSlot;
            slot.slotUnFocused += UnFocusSlot;
            slot.UnFocus();
        }

        confirmPanel.gameObject.SetActive(true);
    }

    protected override void OnOpened()
    {
        confirmPanel.Close();        
        focusedIndex = null;
        selectedIndex = null;
        UpdateFocusHighlight();
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
        UpdateFocusHighlight();

        confirmPanel.Open();
    }

    // ConfirmPanel->ConfirmButton OnClick Event
    public void GameStart()
    {
        Debug.Log((selectedIndex + 1));
        GameManager.Instance.curcharacter = (CharacterId)(selectedIndex + 1);
        SceneLoader.NoLoadingScene("IngameIntro");
    }

    void UpdateFocusHighlight()
    {
        foreach (var slot in slots) slot.UnFocus();
        if (focusedIndex != null)
        {
            slots[(int)focusedIndex].Focus();
        }
    }

    public override void OnUIInputMove(Vector2 dir)
    {
        base.OnUIInputMove(dir);
        // 현재 아무것도 선택되지 않은 상태라면
        if (focusedIndex == null)
        {
            // 왼쪽 → 0번 선택
            if (dir.x < -0.1f) focusedIndex = 0;
            // 오른쪽 → 마지막 선택
            else if (dir.x > 0.1f) focusedIndex = slots.Length - 1;

            UpdateFocusHighlight();
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
        UpdateFocusHighlight();
    }

    public override void OnUIInputConfirm()
    {
        ConfirmSelection();
    }
}

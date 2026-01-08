using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class MainSaveDataPanel : MonoBehaviour, IUIKeyboardTarget
{

    [HideInInspector] public CanvasGroup cg;

    [SerializeField] MainCharacterChoicePanel mainCharacterChoicePanel;

    [SerializeField] MainSaveDataSlot[] slots;
    int? focusedIndex;

    public event Action RequestOpenCharacterSelectPanel;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        foreach (var slot in slots)
        {
            slot.slotFocused += HandleSlotEnter;
            slot.slotUnFocused += HandleSlotExit;
            slot.slotselected += HandleSlotLeftClick;
        }
    }

    void Start()
    {
        // 나중에 세이브 로드 구현시 제대로 하기
        foreach (var slot in slots)
        {
            slot.Bind(null);
        }
    }

    void HandleSlotEnter(int index)
    {
        focusedIndex = index;
        UpdatFocusHighlight();
    }

    void HandleSlotExit(int index)
    {
        focusedIndex = null;
        UpdatFocusHighlight();
    }

    void HandleSlotLeftClick(int index)
    {
        if (slots[index].SaveData == null)
        {
            RequestOpenCharacterSelectPanel?.Invoke();
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

    void UpdatFocusHighlight()
    {
        foreach (var slot in slots)
        {
            slot.UnFocused();
        }
        if (focusedIndex != null)
        {
            slots[(int)focusedIndex].Focused();
        }
    }

    //X 버튼 눌렀을 때
    public void ButtonCancel()
    {
        mainCharacterChoicePanel.Close();
        Close();
    }

    void IUIKeyboardTarget.OnUIMove(Vector2 dir)
    {
        // 현재 아무것도 선택되지 않은 상태라면
        if (focusedIndex == null)
        {
            // 위쪽 → 0번 선택
            if (dir.y > 0.1f) focusedIndex = 0;
            // 아래쪽 → 마지막 선택
            else if (dir.y < -0.1f) focusedIndex = slots.Length - 1;

            UpdatFocusHighlight();
            return;
        }

        //위쪽 방향키시 위쪽 방향으로
        if (dir.y > 0.1f) focusedIndex--;
        //아래쪽 방향키시 아래쪽 방향으로
        else if (dir.y < -0.1f) focusedIndex++;

        //min, max 처리
        if (focusedIndex < 0) focusedIndex = 0;
        else if (focusedIndex >= slots.Length) focusedIndex = slots.Length - 1;

        //강조된 버튼 변경
        UpdatFocusHighlight();
    }

    void IUIKeyboardTarget.OnUIConfirm()
    {
        if (focusedIndex == null) return;
        HandleSlotLeftClick((int)focusedIndex);
    }

    public void OnUICancel()
    {

    }
}

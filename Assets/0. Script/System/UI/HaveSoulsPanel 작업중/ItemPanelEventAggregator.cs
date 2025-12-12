using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemPanelEventAggregator : MonoBehaviour
{
    [Header("Source Info")]
    [SerializeField] StorageTarget source;   // Inventory / EquipInventory / QuestReward 등

    readonly List<IInteractiveView<StoredItem>> views = new List<IInteractiveView<StoredItem>>();

    public IEnumerable<IInteractiveView<StoredItem>> Views => views;
    public StorageTarget Source => source;

    public event Action<ItemUIEventArgs> MouseEntered;
    public event Action<ItemUIEventArgs> MouseExited;
    public event Action<ItemUIEventArgs> RightClicked;
    public event Action<ItemUIEventArgs> DragBegan;
    public event Action<ItemUIEventArgs> Dragging;
    public event Action<ItemUIEventArgs> DragEnded;

    void Awake()
    {
        BuildSlotList();
    }

    void OnEnable()
    {
        SubscribeSlotUI();
    }

    void OnDisable()
    {
        UnSubscribeSlotUI();
    }

    void BuildSlotList()
    {
        views.Clear();

        IInteractiveView<StoredItem>[] found = GetComponentsInChildren<IInteractiveView<StoredItem>>(true);
        for (int i = 0; i < found.Length; i++)
        {
            IInteractiveView<StoredItem> slot = found[i];
            if (slot != null && !views.Contains(slot))
            {
                views.Add(slot);
            }
        }
    }
    public void RebuildSlots()
    {
        UnSubscribeSlotUI();
        BuildSlotList();
        SubscribeSlotUI();
    }

    void SubscribeSlotUI()
    {
        UnSubscribeSlotUI();

        for (int i = 0; i < views.Count; i++)
        {
            IInteractiveView<StoredItem> slot = views[i];
            if (slot == null) continue;

            // Pointer
            if (slot.PointerHandler is UIPointerHandler<StoredItem> pointer)
            {
                pointer.PointerEntered += ForwardMouseEnter;
                pointer.PointerExited += ForwardMouseExit;
            }

            // Click
            if (slot.ClickHandler is UIClickHandler<StoredItem> clicker)
            {
                // slot.ClickHandler.LeftClicked += ...
                clicker.RightClicked += ForwardRightClick;
            }

            // Drag
            if (slot.DragHandler is UIDragHandler<StoredItem> dragger)
            {
                dragger.DragBegan += ForwardBeginDrag;
                dragger.Dragging += ForwardDragging;
                dragger.DragEnded += ForwardDropped;
            }
        }
    }

    void UnSubscribeSlotUI()
    {
        for (int i = 0; i < views.Count; i++)
        {
            IInteractiveView<StoredItem> slot = views[i];
            if (slot == null) continue;

            if (slot.PointerHandler is UIPointerHandler<StoredItem> pointer)
            {
                pointer.PointerEntered -= ForwardMouseEnter;
                pointer.PointerExited -= ForwardMouseExit;
            }

            if (slot.ClickHandler is UIClickHandler<StoredItem> clicker)
            {
                // clicker.LeftClicked += ...
                clicker.RightClicked -= ForwardRightClick;
            }

            if (slot.DragHandler is UIDragHandler<StoredItem> dragger)
            {
                dragger.DragBegan -= ForwardBeginDrag;
                dragger.Dragging -= ForwardDragging;
                dragger.DragEnded -= ForwardDropped;
            }
        }
    }
    
    void ForwardMouseEnter(StoredItem item, RectTransform rect)
        => MouseEntered?.Invoke(new ItemUIEventArgs(item, source, rect, null));

    void ForwardMouseExit()
        => MouseExited?.Invoke(new ItemUIEventArgs(null, source, null, null));

    void ForwardRightClick(StoredItem item)
        => RightClicked?.Invoke(new ItemUIEventArgs(item, source, null, null));

    void ForwardBeginDrag(StoredItem item, PointerEventData e)
        => DragBegan?.Invoke(new ItemUIEventArgs(item, source, null, e));

    void ForwardDragging(PointerEventData e)
        => Dragging?.Invoke(new ItemUIEventArgs(null, source, null, e));

    void ForwardDropped(StoredItem item, PointerEventData e)
        => DragEnded?.Invoke(new ItemUIEventArgs(item, source, null, e));

    // 이벤트 추적용(Debug.log)
    // void ForwardMouseEnter(StoredItem item, RectTransform rect) { Log.Info($"Slot Mouse Enter"); MouseEntered?.Invoke(new ItemUIEventArgs(item, source, rect, null)); }
    // void ForwardMouseExit() { Log.Info($"Slot Mouse Exit"); MouseExited?.Invoke(new ItemUIEventArgs(null, source, null, null)); }
    // void ForwardRightClick(StoredItem item) { Log.Info($"Slot Mouse RightClick"); RightClicked?.Invoke(new ItemUIEventArgs(item, source, null, null)); }
    // void ForwardBeginDrag(StoredItem item, PointerEventData e) { Log.Info($"Slot Mouse DragBegin"); MouseExited?.Invoke(new ItemUIEventArgs(null, source, null, null)); DragBegan?.Invoke(new ItemUIEventArgs(item, source, null, e)); }
    // void ForwardDragging(PointerEventData e) { Log.Info($"Slot Mouse Dragging"); Dragging?.Invoke(new ItemUIEventArgs(null, source, null, e)); }
    // void ForwardDropped(StoredItem item, PointerEventData e) { Log.Info($"Slot Mouse DragEnd"); DragEnded?.Invoke(new ItemUIEventArgs(item, source, null, e)); }
}

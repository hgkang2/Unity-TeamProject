using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class PanelEventAggregator<T> : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] bool buildOnAwake = true;
    [SerializeField] bool subscribeOnEnable = true;

    readonly List<IInteractiveView<T>> views = new List<IInteractiveView<T>>();

    public IEnumerable<IInteractiveView<T>> Views => views;

    public event Action<SlotEventArgs<T>> MouseEntered;
    public event Action<SlotEventArgs<T>> MouseExited;
    public event Action<SlotEventArgs<T>> LeftClicked;
    public event Action<SlotEventArgs<T>> RightClicked;
    public event Action<SlotEventArgs<T>> DragBegan;
    public event Action<SlotEventArgs<T>> Dragging;
    public event Action<SlotEventArgs<T>> DragEnded;

    protected virtual void Awake()
    {
        if (buildOnAwake)
        {
            BuildViewList();
        }
    }

    protected virtual void OnEnable()
    {
        if (subscribeOnEnable)
        {
            SubscribeViews();
        }
    }

    protected virtual void OnDisable()
    {
        UnsubscribeViews();
    }

    protected void BuildViewList()
    {
        
        views.Clear();

        // 인터페이스라 GetComponentsInChildren<IInteractiveView<T>>()가 안 먹을 수도 있으므로
        // MonoBehaviour 전체 긁어서 캐스팅
        MonoBehaviour[] found = GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < found.Length; i++)
        {
            MonoBehaviour mb = found[i];
            IInteractiveView<T> view = mb as IInteractiveView<T>;
            if (view != null && views.Contains(view) == false)
            {
                //Debug.Log($"Build View List. {mb.name}");
                views.Add(view);
            }
        }
    }

    public void RebuildViews()
    {
        UnsubscribeViews();
        BuildViewList();
        SubscribeViews();
    }

    protected void SubscribeViews()
    {
        UnsubscribeViews();

        for (int i = 0; i < views.Count; i++)
        {
            IInteractiveView<T> view = views[i];
            if (view == null)
            {
                continue;
            }

            // Pointer
            UIPointerHandler<T> pointer = view.PointerHandler as UIPointerHandler<T>;
            if (pointer != null)
            {
                pointer.PointerEntered += OnPointerEntered;
                pointer.PointerExited += OnPointerExited;
            }

            // Click
            UIClickHandler<T> click = view.ClickHandler as UIClickHandler<T>;
            if (click != null)
            {
                click.LeftClicked += OnLeftClicked;
                click.RightClicked += OnRightClicked;
            }

            // Drag
            UIDragHandler<T> drag = view.DragHandler as UIDragHandler<T>;
            if (drag != null)
            {
                drag.DragBegan += OnDragBegan;
                drag.Dragging += OnDragging;
                drag.DragEnded += OnDragEnded;
            }
        }
    }

    protected void UnsubscribeViews()
    {
        for (int i = 0; i < views.Count; i++)
        {
            IInteractiveView<T> view = views[i];
            if (view == null)
            {
                continue;
            }

            UIPointerHandler<T> pointer = view.PointerHandler as UIPointerHandler<T>;
            if (pointer != null)
            {
                pointer.PointerEntered -= OnPointerEntered;
                pointer.PointerExited -= OnPointerExited;
            }

            UIClickHandler<T> click = view.ClickHandler as UIClickHandler<T>;
            if (click != null)
            {
                click.LeftClicked -= OnLeftClicked;
                click.RightClicked -= OnRightClicked;
            }

            UIDragHandler<T> drag = view.DragHandler as UIDragHandler<T>;
            if (drag != null)
            {
                drag.DragBegan -= OnDragBegan;
                drag.Dragging -= OnDragging;
                drag.DragEnded -= OnDragEnded;
            }
        }
    }

    // === Handlers ===

    void OnPointerEntered(T data, RectTransform rect, PointerEventData eventData)
    {
        if (MouseEntered == null)
        {
            return;
        }

        SlotEventArgs<T> args = new SlotEventArgs<T>(data, rect, eventData);
        MouseEntered.Invoke(args);
    }

    void OnPointerExited()
    {
        if (MouseExited == null)
        {
            return;
        }

        SlotEventArgs<T> args = new SlotEventArgs<T>(default(T), null, null);
        MouseExited.Invoke(args);
    }

    void OnLeftClicked(T data)
    {
        if (LeftClicked == null)
        {
            return;
        }

        SlotEventArgs<T> args = new SlotEventArgs<T>(data, null, null);
        LeftClicked.Invoke(args);
    }

    void OnRightClicked(T data)
    {
        if (RightClicked == null)
        {
            return;
        }

        SlotEventArgs<T> args = new SlotEventArgs<T>(data, null, null);
        RightClicked.Invoke(args);
    }

    void OnDragBegan(T data, PointerEventData eventData)
    {
        if (DragBegan == null)
        {
            return;
        }

        SlotEventArgs<T> args = new SlotEventArgs<T>(data, null, eventData);
        DragBegan.Invoke(args);
    }

    void OnDragging(PointerEventData eventData)
    {
        if (Dragging == null)
        {
            return;
        }

        SlotEventArgs<T> args = new SlotEventArgs<T>(default(T), null, eventData);
        Dragging.Invoke(args);
    }

    void OnDragEnded(T data, PointerEventData eventData)
    {
        if (DragEnded == null)
        {
            return;
        }

        SlotEventArgs<T> args = new SlotEventArgs<T>(data, null, eventData);
        DragEnded.Invoke(args);
    }
}

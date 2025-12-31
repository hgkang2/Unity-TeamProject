using System;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    Player player;

    [Header("Scan (2D)")]
    [SerializeField] float overlapRadius = 1.2f;
    [SerializeField] Vector2 offset = Vector2.zero;
    [SerializeField] LayerMask interactableLayer;

    public bool gizmoEnable = true;

    public IInteractable selected;
    public IInteractable current;

    static readonly Collider2D[] buffer = new Collider2D[32];

    public event Action<IInteractable> SelectedChanged;

    void Awake() {
        player = GetComponent<Player>();
        InputManager.Instance.InteractPressed += Interact;
    }

    float timer;
    void Update()
    {
        if(Time.time > timer + 0.2f)
        {
            timer = Time.time;
            Scan();
        }
    }

    void OnDestroy()
    {
        InputManager.Instance.InteractPressed -= Interact;
    }

    public void Scan()
    {
        // 상호작용 중이면 선택 갱신 중단 + UI 정리
        if (current != null)
        {
            if (selected != null)
            {
                selected.OnUnfocus();
                selected = null;
                SelectedChanged?.Invoke(null);
            }
            return;
        }

        Vector2 scanCenter = (Vector2)transform.position + offset;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = interactableLayer;
        filter.useTriggers = true; // 트리거 콜라이더도 감지하고 싶으면 true, 아니면 false

        int count = Physics2D.OverlapCircle(scanCenter, overlapRadius, filter, buffer);

        IInteractable best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = buffer[i];
            if (col == null) continue;

            IInteractable cand = col.GetComponentInParent<IInteractable>();
            if (cand == null) continue;

            if (!cand.CanInteract()) continue;

            // "현재 위치와 가장 가까운 것" 기준
            Vector2 p = col.bounds.center;
            float sqr = (p - (Vector2)transform.position).sqrMagnitude;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = cand;
            }
        }

        if (!ReferenceEquals(selected, best))
        {
            selected?.OnUnfocus();
            selected = best;
            selected?.OnFocus();
            SelectedChanged?.Invoke(selected);
        }
    }

    public void Interact()
    {
        if (current != null)
        {
            InteractExit();
            return;
        }

        if (selected == null) return;
        if (!selected.IsAvailable()) return;

        current = selected;
        current.Interact(player);
    }

    public void InteractExit()
    {
        if (current == null) return;
        current.Exit();
        current = null;
    }

    public void Clear()
    {
        if (selected != null)
        {
            selected.OnUnfocus();
            selected = null;
        }
        SelectedChanged?.Invoke(null);
    }

    void OnDrawGizmos()
    {
        if (!gizmoEnable) return;

        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + (Vector3)offset;
        Gizmos.DrawWireSphere(center, overlapRadius);
    }
}

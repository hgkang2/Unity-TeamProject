using System;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Scan (2D)")]
    [SerializeField] float overlapRadius = 1.2f;
    [SerializeField] Vector2 offset = Vector2.zero;
    [SerializeField] LayerMask interactableLayer;

    public bool gizmoEnable = true;

    public IInteractable selected;
    public IInteractable current;

    static readonly Collider2D[] buffer = new Collider2D[32];

    public event Action<IInteractable> SelectedChanged;

    [Obsolete]
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
        int count = Physics2D.OverlapCircleNonAlloc(scanCenter, overlapRadius, buffer, interactableLayer);

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

    public bool Interact(Player player)
    {
        if (current != null)
        {
            InteractExit();
            return true;
        }

        if (selected == null) return false;
        if (!selected.IsAvailable()) return false;

        current = selected;
        current.Interact(player);
        return true;
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

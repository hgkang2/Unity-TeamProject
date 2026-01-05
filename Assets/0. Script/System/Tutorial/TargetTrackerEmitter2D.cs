using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TargetTrackerEmitter2D : MonoBehaviour
{
    [SerializeField] Transform target;

    public event Action TargetEntered;

    Collider2D col;
    bool triggered;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        triggered = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (target == null)
            return;

        if (other.transform != target)
            return;

        triggered = true;
        TargetEntered?.Invoke();
    }
}

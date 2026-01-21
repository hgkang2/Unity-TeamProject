using System;
using UnityEngine;

public class TutorialTrigger2D : MonoBehaviour
{
    public event Action Triggered;

    bool oneShot = true;
    bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        Triggered?.Invoke();

        if (oneShot) gameObject.SetActive(false);
    }
}
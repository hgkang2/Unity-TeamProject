using UnityEngine;
using UnityEngine.EventSystems;


public class SettingItemDragFocusLockRelay : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    SettingPanel panel;

    void Awake()
    {
        panel = GetComponentInParent<SettingPanel>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        panel?.SetFocusLocked(true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        panel?.SetFocusLocked(false);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class SettingItemHoverRelay : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    SettingPanel panel;
    ISettingItem item;

    void Awake()
    {
        // 부모에서 패널 찾기
        panel = GetComponentInParent<SettingPanel>();

        // 같은 오브젝트(또는 자식)에 ISettingItem이 붙어있는 경우도 있으니 둘 다 시도
        item = GetComponent<ISettingItem>();
        if (item == null)
            item = GetComponentInChildren<ISettingItem>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        panel?.RequestFocus(item);
    }

    // hover가 아니라 클릭/드래그로 들어와도 마지막 이벤트 기준으로 포커스 맞추기
    public void OnPointerDown(PointerEventData eventData)
    {
        panel?.RequestFocus(item);
    }
}

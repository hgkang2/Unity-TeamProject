using UnityEngine;
using UnityEngine.UI;

public class HaveSoulSlot : MonoBehaviour, IInteractiveView<SoulData>
{
    [SerializeField] Image soulIcon;
    SoulData data;

    public UIPointerHandler PointerHandler { get; private set; }
    public UIClickHandler ClickHandler { get; private set; }
    public UIDragHandler DragHandler => null;

    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        UIPointerHandler<SoulData> pointerT = GetComponent<UIPointerHandler<SoulData>>();
        UIClickHandler<SoulData> clickT = GetComponent<UIClickHandler<SoulData>>();

        PointerHandler = pointerT;
        ClickHandler = clickT;

        if (pointerT != null)
        {
            pointerT.GetData = () => data;
            pointerT.GetRect = () => Rect;
        }

        if (clickT != null)
        {
            clickT.GetData = () => data;
        }

        Clear();
    }

    public void Bind(SoulData data)
    {
        soulIcon.gameObject.SetActive(true);
        soulIcon.sprite = data.soulIcon;
    }

    public void Clear()
    {
        soulIcon.sprite = null;
        soulIcon.gameObject.SetActive(false);
    }
}

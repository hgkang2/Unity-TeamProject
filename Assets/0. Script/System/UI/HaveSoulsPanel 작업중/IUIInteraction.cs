// 플레이어와 상호작용 할 수 있는 UI
public interface IUIInteraction
{
    public UIPointerHandler PointerHandler { get; }
    public UIClickHandler ClickHandler { get; }
    public UIDragHandler DragHandler { get; }
}

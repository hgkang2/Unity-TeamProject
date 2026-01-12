public interface ISettingItem
{
    void SetSelected(bool selected);

    bool CanAdjust { get; }
    void Adjust(int dir);   // -1 : Left, +1 : Right

    bool CanSubmit { get; }
    void Submit();
}

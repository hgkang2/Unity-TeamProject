public interface IOpenStackUI
{
    bool PauseGame { get; }   // 이 UI가 최상단이면 Pause?
    void Show();
    void Hide();
}
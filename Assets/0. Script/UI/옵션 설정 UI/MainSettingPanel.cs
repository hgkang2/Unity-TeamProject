using UnityEngine;

public class MainSettingPanel : SettingPanel
{
    protected override int GetDefaultIndex() => 0;

    protected override void BuildNavMap()
    {
        navMap.Clear();

        // vertical only (0~4)
        navMap[0] = new Nav4(up: 4, down: 1, left: 0, right: 0);
        navMap[1] = new Nav4(up: 0, down: 2, left: 1, right: 1);
        navMap[2] = new Nav4(up: 1, down: 3, left: 2, right: 2);
        navMap[3] = new Nav4(up: 2, down: 4, left: 3, right: 3);
        navMap[4] = new Nav4(up: 3, down: 0, left: 4, right: 4);
    }

    protected override void OnOpened() { }
    protected override void OnClosing() { }
}

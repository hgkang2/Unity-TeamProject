using UnityEngine;

public class IngameSettingPanel : SettingPanel
{
    protected override int GetDefaultIndex() => 4;

    protected override void BuildNavMap()
    {
                navMap.Clear();

        // 0(BGM)
        navMap[0] = new Nav4(up: 5, down: 1, left: 0, right: 0);
        // 1(SFX)
        navMap[1] = new Nav4(up: 0, down: 2, left: 1, right: 1);
        // 2(해상도)
        navMap[2] = new Nav4(up: 1, down: 3, left: 2, right: 2);

        // 1행 버튼: 3(적용) 4(저장)
        navMap[3] = new Nav4(up: 2, down: 5, left: 3, right: 4);
        navMap[4] = new Nav4(up: 2, down: 6, left: 3, right: 4);

        // 2행 버튼: 5(메인) 6(종료)
        navMap[5] = new Nav4(up: 3, down: 0, left: 5, right: 6);
        navMap[6] = new Nav4(up: 4, down: 0, left: 5, right: 6);
    }

    protected override void OnOpened()
    {
        base.OnOpened();
        TimeManager.Pause();
    }

    protected override void OnClosing()
    {
        TimeManager.Resume();
        base.OnClosing();
    }
}

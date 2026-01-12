using UnityEngine;

public class EscPanel : UIPanelBase
{
    SettingPanel settingPanel;

    protected override void Init()
    {
        SceneContext sc = FindFirstObjectByType<SceneContext>();
        settingPanel = sc.settingPanel;
        settingPanel.gameObject.SetActive(true);
        settingPanel.Close();
    }

    public void OpenSettingPanel()
    {
        settingPanel.Open();
    }

    protected override void OnClosing()
    {
        TimeManager.Resume();
    }
}

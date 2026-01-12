using UnityEngine;

public class EscPanel : UIPanelBase
{
    
    protected override void OnClosing()
    {
        TimeManager.Resume();
    }
}

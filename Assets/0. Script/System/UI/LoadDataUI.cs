using UnityEngine;

public class LoadDataUI : UIKeyboardHandler
{
    protected override void OnUIMove(Vector2 dir)
    {
        
    }
    protected override void OnUICancel()
    {
        base.OnUICancel();
    }

    protected override void OnUIConfirm()
    {
        base.OnUIConfirm();
    }

    public void ToStageScene()
    {
        SoundManager.Instance.StopBGM();
        SceneLoader.NoLoadingScene("CharacterChoice");
    }
}

using UnityEngine;

public class LoadDataUI : MonoBehaviour
{

    public void ToStageScene()
    {
        SoundManager.Instance.StopBGM();
        SceneLoader.NoLoadingScene("CharacterChoice");
    }
}

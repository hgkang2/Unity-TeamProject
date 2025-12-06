using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    static SceneLoader instance;

    string nextSceneName;

    public static string NextSceneName
    {
        get { return instance != null ? instance.nextSceneName : null; }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void LoadScene(string targetSceneName)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("SceneLoader");
            instance = go.AddComponent<SceneLoader>();
            DontDestroyOnLoad(go);
        }

        instance.nextSceneName = targetSceneName;

        // 로딩 화면 씬으로 먼저 이동
        SceneManager.LoadScene("Loading");
    }
    public static void NoLoadingScene(string targetSceneName){
        SceneManager.LoadScene(targetSceneName);
    }
}

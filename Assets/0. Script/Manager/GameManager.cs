using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(TutorialRunner))]
public class GameManager : MonoBehaviour
{
    [SerializeField] CanvasGroup fade;
    public CharacterId curcharacter = CharacterId.None;
    TutorialRunner tutorialRunner;

    public float ingameTime;
    public float stageTime;

    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            // 없으면 자동 생성
            if (instance == null)
            {
                GameObject obj = new GameObject(nameof(GameManager));
                instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
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

        SettingsManager.LoadOrDefault();
        
        tutorialRunner = GetComponent<TutorialRunner>();
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "IngameIntro":
                SoundManager.Instance.StopBGM();
                fade.alpha = 1;
                StartCoroutine(FadeInRoutine(3));
                break;
            case "Stage1":
                tutorialRunner.StartTutorial();
                ingameTime = 0;
                stageTime = 0;
                if (SoundManager.Instance == null) return;
                SoundManager.Instance.PlayBGM("Stage1");
                break;
            case "Stage1_Test":
                ingameTime = 0;
                stageTime = 0;
                tutorialRunner.StartTutorial();
                if (SoundManager.Instance == null) return;
                SoundManager.Instance.PlayBGM("Stage1");
                break;
        }
    }

    void Update()
    {
        ingameTime += Time.deltaTime;
        stageTime += Time.deltaTime;
    }

    public IEnumerator TeleportRoutine(Player p, Transform targetPosition)
    {

        TimeManager.Pause();
        yield return FadeOutRoutine(1f);   // 끝날 때까지 대기

        Vector3 oldPos = p.transform.position;
        Vector3 newPos = targetPosition.position;
        Vector3 delta = newPos - oldPos;

        p.transform.position = newPos;
        CameraManager.Instance.CameraWarp(p.transform, delta);

        TimeManager.Resume();
        yield return FadeInRoutine(1f);    // 끝날 때까지 대기
    }

    IEnumerator FadeOutRoutine(float duration)
    {
        fade.blocksRaycasts = true;

        float t = 0f;
        float start = fade.alpha;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fade.alpha = Mathf.Lerp(start, 1f, t / duration);
            yield return null;
        }

        fade.alpha = 1f;
    }

    IEnumerator FadeInRoutine(float duration)
    {
        float t = 0f;
        float start = fade.alpha;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fade.alpha = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }

        fade.alpha = 0f;
        fade.blocksRaycasts = false;
    }

    public void SetCharacter(int num)
    {
        // 1 : serena, 2 : luna, 3 : 미출시
        curcharacter = (CharacterId)(num);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

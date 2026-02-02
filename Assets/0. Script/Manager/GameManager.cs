using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(TutorialRunner))]
public class GameManager : MonoBehaviour
{
    [SerializeField] CanvasGroup fade;
    public CharacterId curcharacter = CharacterId.None;
    TutorialRunner tutorialRunner;

    public float ingameTime;
    public float stageTime;

    static GameManager instance;
    public static GameManager Instance => instance;

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
                InitInGame();
                if (SoundManager.Instance == null) return;
                SoundManager.Instance.PlayBGM("Stage1");
                break;
            case "Stage1_Test":
                InitInGame();
                if (SoundManager.Instance == null) return;
                SoundManager.Instance.PlayBGM("Stage1");
                break;
        }
    }

    void Update()
    {
        ingameTime += Time.unscaledDeltaTime;
        stageTime += Time.unscaledDeltaTime;
    }

    public void InitInGame()
    {
        ingameTime = 0;
        stageTime = 0;

        SoulManager.Instance.CurSouls.Clear();

        hasFlame = 0;
        usedFlame = 0;
    }

    public IEnumerator TeleportRoutine(Player p, Transform targetPosition, BGSetKey BGKey)
    {

        TimeManager.Pause();
        yield return FadeOutRoutine(1f);   // 끝날 때까지 대기

        Vector3 oldPos = p.transform.position;
        Vector3 newPos = targetPosition.position;
        Vector3 delta = newPos - oldPos;

        p.transform.position = newPos;
        CameraManager.Instance.CameraWarp(p.transform, delta);

        InGameFollowBGManager.Instance.ChangeIngameBG(BGKey);
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

    #region 정화의 불꽃 처리
    int hasFlame;
    public int HasFlame => hasFlame;
    public event Action changedHasFlame;
    public void AddFlame(int amount = 1)
    {
        if (amount <= 0) return;

        hasFlame += amount;
        changedHasFlame?.Invoke();
    }

    int usedFlame;
    public int UsedFlame => usedFlame;
    public event Action changedUsedFlame;  
    public void AlterPurify(int amount)
    {
        usedFlame += amount;
        hasFlame -= amount;

        // 바쳐진 제물의 개수에 따라 확률적으로 제단 활성화
        switch (usedFlame)
        {
            case 0: return;
            case 1:
                if (UnityEngine.Random.value < 0.3f) ActivateAlter();
                break;
            case 2:
                if (UnityEngine.Random.value < 0.6f) ActivateAlter();
                break;
            case 3:
                ActivateAlter();
                break;
            default:
                Debug.LogWarning($"4개 이상의 제물이 바쳐짐");
                break;
        }

        changedHasFlame?.Invoke();
        changedUsedFlame?.Invoke();
    }

    public event Action AltarActivated;
    public void ActivateAlter()
    {
        usedFlame = 3;
        changedUsedFlame?.Invoke();
        AltarActivated?.Invoke();
        Debug.Log("제단 활성화");
    }

    #endregion

    #region 인게임 끝
    public void PlayerDieGotoMain()
    {
        SoundManager.Instance.StopBGM();
        SceneLoader.LoadScene("Start");
    }
    #endregion

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

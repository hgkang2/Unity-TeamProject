using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IntroSkip : MonoBehaviour
{
    [SerializeField] Image fillImage;
    [SerializeField] float holdSeconds = 1.5f;
    [SerializeField] KeyCode key = KeyCode.Escape;
    [SerializeField] AudioSource audioSource;
    [SerializeField] string GotoSceneName;
    public float autoSkipTime;
    float EnableTime;
    float holdTime;
    bool isStarting;

    void OnEnable()
    {
        EnableTime = Time.time;
        Cursor.visible = false;
    }
    void OnDisable()
    {
        Cursor.visible = true;
    }
    void Update()
    {
        if (isStarting)
            return;

        if (Input.GetKey(key))
        {
            holdTime += Time.unscaledDeltaTime;
        }
        else
        {
            holdTime = 0f;
        }

        float t = Mathf.Clamp01(holdTime / holdSeconds);
        fillImage.fillAmount = t;

        if(t >= 1f)
        {
            StartGame();
        }
        
        if (Time.time > EnableTime + autoSkipTime)
        {
            StartCoroutine(GameStartRoutine());
        }
    }

    System.Collections.IEnumerator GameStartRoutine()
    {
        isStarting = true;

        // 오디오 페이드 아웃
        if (audioSource != null)
        {
            audioSource.DOFade(0f, 3).SetUpdate(true); // unscaled
        }

        // 3초(또는 설정값) 대기 후 씬 로드
        yield return new WaitForSecondsRealtime(3);

        StartGame();
    }
    void StartGame()
    {
        SceneLoader.LoadScene(GotoSceneName);
    }
}

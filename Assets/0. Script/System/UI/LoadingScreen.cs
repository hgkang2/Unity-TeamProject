using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] ProgressBar progressBar;
    [SerializeField] float minDisplayTime = 1.0f; // 로딩이 너무 빨리 끝나도 최소 1초는 보여주기

    void Start()
    {
        StartCoroutine(LoadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        string targetScene = SceneLoader.NextSceneName;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("SceneLoader.NextSceneName is null or empty.");
            yield break;
        }

        float startTime = Time.time;

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            // op.progress는 0~0.9 사이까지만 올라감
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            Debug.Log($"{progress}");

            if (progressBar != null)
            {
                progressBar.SetRatio(progress);
            }

            // 0.9 이상이면 거의 로딩 끝
            if (op.progress >= 0.9f)
            {
                // 최소 노출 시간 보장
                if (Time.time - startTime >= minDisplayTime)
                {
                    op.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}

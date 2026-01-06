using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    static CameraManager instance;
    public static CameraManager Instance => instance;

    [Header("Cinemachine Settings")]
    CinemachineCamera cinemachineCamera;

    CinemachineBasicMultiChannelPerlin perlin;
    Coroutine shakeRoutine;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
        cinemachineCamera = sceneContext.cinemachineCamera;
    }

    // ------------------------------------------------------
    // 카메라 흔들기 (피격, 폭발, 강공격 등)
    // ------------------------------------------------------
    public void Shake(float amplitude, float frequency, float duration)
    {
        if (perlin == null) return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake(amplitude, frequency, duration));
    }

    IEnumerator DoShake(float amplitude, float frequency, float duration)
    {
        perlin.AmplitudeGain = amplitude;
        perlin.FrequencyGain = frequency;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
        shakeRoutine = null;
    }

    public void CameraWarp(Transform target, Vector3 delta)
    {
        cinemachineCamera.OnTargetObjectWarped(target, delta);
    }

    public void Zoom(float targetFOV, float duration)
    {
        if (cinemachineCamera == null) return;
        StartCoroutine(DoZoom(targetFOV, duration));
    }

    IEnumerator DoZoom(float targetFOV, float duration)
    {
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }
    }
}

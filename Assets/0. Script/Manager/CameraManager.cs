using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System;

public class CameraManager : MonoBehaviour
{
    static CameraManager instance;
    public static CameraManager Instance => instance;

    CinemachineBrain brain; // Main Camera에 붙은 CinemachineBrain
    CinemachineCamera cinemachineCamera;
    CinemachineCamera cinemachineCamera_tutorialTrap;  // 추가(함정 고정)

    CinemachineBasicMultiChannelPerlin perlin;
    Coroutine shakeRoutine;

    public event Action CinemachineSequenceFinished;

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
        brain = Camera.main.GetComponent<CinemachineBrain>();
        defaultChannelMask = brain.ChannelMask;
        cinemachineCamera = sceneContext.cinemachineCamera;
        cinemachineCamera_tutorialTrap = sceneContext.cinemachineCamera_tutorialTrap;
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

    Coroutine channelRoutine;
    OutputChannels defaultChannelMask;

    IEnumerator CinemachineCameraBlendRoutine(CinemachineChannel channel, float holdSeconds)
    {
        // 타겟 채널만 허용
        brain.ChannelMask =
            (OutputChannels)(1 << (int)channel);

        if (holdSeconds > 0f)
            yield return new WaitForSecondsRealtime(holdSeconds);

        // 원래 채널로 복귀
        brain.ChannelMask = defaultChannelMask;

        channelRoutine = null;

        CinemachineSequenceFinished?.Invoke();
    }
    public void ChangeCinemachine(CinemachineChannel channel, float holdSeconds)
    {
        if (brain == null) return;

        if (channelRoutine != null)
            StopCoroutine(channelRoutine);

        channelRoutine = StartCoroutine(
            CinemachineCameraBlendRoutine(channel, holdSeconds)
        );
    }

    public void ChangeCinemachineTutorialTrap(float holdSeconds = 2f)
    {
        ChangeCinemachine(CinemachineChannel.TutorialTrap, holdSeconds);
    }

}

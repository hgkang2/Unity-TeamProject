using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    static CameraManager instance;
    public static CameraManager Instance => instance;

    [Header("Cinemachine Settings")]
    [SerializeField] CinemachineCamera cinemachineCam;   // 인스펙터에서 물려주면 제일 좋음

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

        // 인스펙터에 안 물려있으면 씬에서 첫 번째 CinemachineCamera 찾기
        if (cinemachineCam == null)
        {
            cinemachineCam = FindFirstObjectByType<CinemachineCamera>();
            // 필요하면 비활성 포함으로:
            // cinemachineCam = FindFirstObjectByType<CinemachineCamera>(FindObjectsInactive.Include);
        }

        if (cinemachineCam != null)
        {
            // Cinemachine 3.x에선 Noise 컴포넌트가 카메라 게임오브젝트에 직접 붙음
            perlin = cinemachineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
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

    // ------------------------------------------------------
    // 나중에 써먹을 줌 기능 예시
    // ------------------------------------------------------
    public void Zoom(float targetFOV, float duration)
    {
        if (cinemachineCam == null) return;
        StartCoroutine(DoZoom(targetFOV, duration));
    }

    IEnumerator DoZoom(float targetFOV, float duration)
    {
        float startFOV = cinemachineCam.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cinemachineCam.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }
    }
}

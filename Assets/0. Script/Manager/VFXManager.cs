using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VFXManager : MonoBehaviour
{
    static VFXManager vfxManager;
    public static VFXManager Instance => vfxManager;

    [SerializeField] ParticleSystem clickVFX;
    [SerializeField] Camera vfxCamera;
    public Camera VFXCamera => vfxCamera;
    Camera mainCamera;

    void Awake()
    {
        // 싱글톤
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        vfxManager = this;
        DontDestroyOnLoad(gameObject);

        RefreshCameras();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 혹시 처음 씬에서도 제대로 못 잡았으면 한 번 더
        RefreshCameras();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 바뀔 때마다 새 카메라들 다시 물어오기
        RefreshCameras();
    }

    void RefreshCameras()
    {
        // 메인 카메라 다시 찾기
        if (mainCamera == null || !mainCamera)
            mainCamera = Camera.main;

        // VFX 카메라 다시 찾기 
        if (vfxCamera == null || !vfxCamera)
        {
            GameObject camObj = GameObject.FindWithTag("VFXCamera");
            if (camObj != null)
                vfxCamera = camObj.GetComponent<Camera>();
        }
    }

    void LateUpdate()
    {
        // 씬 초기화 타이밍 등에서 아직 못 잡았으면 다시 시도
        if (mainCamera == null || !mainCamera || vfxCamera == null || !vfxCamera)
        {
            RefreshCameras();
            if (mainCamera == null || !mainCamera || vfxCamera == null || !vfxCamera)
                return; // 여전히 없으면 이번 프레임은 패스
        }

        vfxCamera.transform.position = mainCamera.transform.position;
        // 필요하면 회전/사이즈도 맞춰주기:
        // vfxCamera.rotation = mainCamera.transform.rotation;
    }

    public void MouseClickVFX()
    {
        Vector2 clickPos = InputManager.Instance.GetMouseWorldPos();
        ParticleSystem vfx = Instantiate(clickVFX, clickPos, Quaternion.identity);
        var vfxMain = vfx.main;
        vfxMain.useUnscaledTime = true;
        vfx.Simulate(0f, true, true);
        vfx.Play();
        StartCoroutine(DestroyAfterUnscaled(vfx.gameObject, vfxMain.duration));
    }

    IEnumerator DestroyAfterUnscaled(GameObject go, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // 타임스케일 무시
            yield return null;
        }
        Destroy(go);
    }
}

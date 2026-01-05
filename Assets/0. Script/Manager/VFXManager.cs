using System.Collections;
using System.Collections.Generic;
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


    [Header("Attack Sprite VFX")]
    [SerializeField] AttackVFXLibrary attackVFXLibrary;
    [SerializeField] Transform attackPoolRoot;
    [SerializeField] int prewarmEach = 3;
    Dictionary<GameObject, GameObjectPool> poolsByPrefab = new Dictionary<GameObject, GameObjectPool>();

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
    #region 공격이펙트풀링
    public void PlayAttackSpriteVFX(
        string key,
        Transform followParent,
        Vector3 position,
        Quaternion rotation)
    {
        if (attackVFXLibrary == null) return;

        AttackVFXLibrary.Entry entry = attackVFXLibrary.Get(key);
        if (entry == null || entry.prefab == null)
        {
            Debug.LogWarning($"AttackVFX key not found: {key}");
            return;
        }

        GameObjectPool pool = GetOrCreatePool(entry.prefab);
        GameObject go = pool.Rent();

        if (entry.followOwner && followParent != null)
        {
            // 1) 따라가는 VFX
            //    부모에 붙이고 로컬 기준으로 관리
            go.transform.SetParent(followParent, false);

            // 월드 기준 위치/회전을 그대로 맞추고 싶으면 이렇게
            go.transform.localPosition = entry.localOffset;
            go.transform.rotation = rotation;

            // 또는, followParent 기준 localOffset/localRotation을 쓰는 구조라면
            // go.transform.localPosition = entry.localOffset;
            // go.transform.localRotation = entry.localRotation;
        }
        else
        {
            // 2) 월드 고정 VFX
            go.transform.SetParent(null, false);
            Vector3 worldPos = position + rotation * entry.localOffset;
            Quaternion worldRot = rotation * Quaternion.Euler(entry.localEuler);
            go.transform.localScale = followParent.lossyScale;

            go.transform.SetPositionAndRotation(worldPos, worldRot);
        }

        // 3) 활성화 → OnEnable에서 애니메이션 재생
        go.SetActive(true);
    }



    GameObjectPool GetOrCreatePool(GameObject prefab)
    {
        GameObjectPool pool;
        if (poolsByPrefab.TryGetValue(prefab, out pool))
        {
            return pool;
        }

        pool = new GameObjectPool(prefab, attackPoolRoot, prewarmEach);
        poolsByPrefab.Add(prefab, pool);
        return pool;
    }
    #endregion
}

using System.Collections;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] ParticleSystem clickVFX;
    [SerializeField] Transform vfxCamera;
    Camera mainCamera;
    void Awake()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        vfxCamera.position = mainCamera.transform.position;
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

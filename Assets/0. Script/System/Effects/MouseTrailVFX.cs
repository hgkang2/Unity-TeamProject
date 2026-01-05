using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTrailVFX : MonoBehaviour
{
    ParticleSystem trailVFX;

    [SerializeField] float tailDuration;     // 멈춘 뒤 몇 초 동안 유지
    [SerializeField] float moveThreshold;      // 이 이상 움직여야 "움직임"으로 인정


    Vector3 lastMousePosition;
    float lastMoveTime;
    bool initialized;

    void Start()
    {
        trailVFX = GetComponent<ParticleSystem>();

        var vfxMain = trailVFX.main;
        vfxMain.useUnscaledTime = true;
        trailVFX.Play();

        lastMoveTime = Time.time;
        initialized = true;
    }

    void Update()
    {
        if (!initialized)
        {
            return;
        }
        if(VFXManager.Instance.VFXCamera == null) return;

        // 1. 마우스 이동량 (픽셀 단위) 읽기
        Vector2 delta = Mouse.current.delta.ReadValue();
        bool isMoving = delta.sqrMagnitude > moveThreshold * moveThreshold;


        if (isMoving)
        {
            lastMoveTime = Time.unscaledTime;
        }

        // 2. 움직이고 있거나, tailDuration 이내면 계속 뿜기
        float now = Time.unscaledTime;         // ← 그리고 여기
        bool shouldEmit = isMoving || (now - lastMoveTime <= tailDuration);

        // 3. 파티클 위치를 마우스 위치로
        transform.position = GetMouseWorldPos();

        // 4. Emission On/Off
        ParticleSystem.EmissionModule emission = trailVFX.emission;
        emission.enabled = shouldEmit;
    }
    Vector3 GetMouseWorldPos()
    {
        // 화면 좌표 → VFXCamera 기준 월드 좌표
        Vector2 screenPos = Mouse.current.position.ReadValue();
        // 카메라 앞쪽 얼마만큼 떨어뜨릴지 (Orthographic이면 그냥 양수면 됨)

        Vector3 pos = new Vector3(screenPos.x, screenPos.y, 10f);
        return VFXManager.Instance.VFXCamera.ScreenToWorldPoint(pos);
    }
}

using UnityEngine;

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

        trailVFX.Play();

        lastMousePosition = InputManager.Instance.GetMouseWorldPos();
        lastMoveTime = Time.time;
        initialized = true;
    }

    void Update()
    {
        if (!initialized)
        {
            return;
        }

        // 1. 현재 마우스 월드 위치 읽기
        Vector3 currentMousePos = InputManager.Instance.GetMouseWorldPos();

        // 2. 이전 프레임과 거리 차이
        float deltaSqr = (currentMousePos - lastMousePosition).sqrMagnitude;
        bool isMoving = deltaSqr > moveThreshold * moveThreshold;

        if (isMoving)
        {
            lastMoveTime = Time.time;
        }

        // 3. 움직이고 있거나, tailDuration 이내면 계속 뿜기
        bool shouldEmit = isMoving || (Time.time - lastMoveTime <= tailDuration);

        // 4. 파티클 위치를 마우스 위치로
        transform.position = currentMousePos;

        // 5. Emission On/Off
        ParticleSystem.EmissionModule emission = trailVFX.emission;
        emission.enabled = shouldEmit;

        // 6. 마지막 위치 갱신 (제일 마지막!)
        lastMousePosition = currentMousePos;
    }
}

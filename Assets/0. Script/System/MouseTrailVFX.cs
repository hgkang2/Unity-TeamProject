using UnityEngine;

public class MouseTrailVFX : MonoBehaviour
{
    ParticleSystem trailVFX;
    Camera mainCamera;
    [SerializeField] float tailDuration = .25f;      // 멈춘 뒤 몇 초 동안 유지할지
    [SerializeField] float moveThreshold = 0.1f;   // 이 이상 움직여야 "움직임"으로 인정
    [SerializeField] float zDistanceFromCamera = 5f; // 카메라로부터 얼마나 떨어진 z에 둘지

    Vector3 lastMousePosition;
    float lastMoveTime;
    bool initialized;

    void Start()
    {
        trailVFX = GetComponent<ParticleSystem>();
        mainCamera = Camera.main;
        trailVFX.Play();

        lastMousePosition = Input.mousePosition;
        lastMoveTime = Time.time;
        initialized = true;
    }

    void Update()
    {
        if (!initialized || trailVFX == null || mainCamera == null)
        {
            return;
        }

        Vector3 currentMousePos = Input.mousePosition;

        // 1. 마우스 이동량 체크
        float delta = (currentMousePos - lastMousePosition).sqrMagnitude;
        bool isMoving = delta > moveThreshold * moveThreshold;

        if (isMoving)
        {
            lastMoveTime = Time.time;
        }

        // 2. "지금 파티클이 켜져 있어야 하는가?"
        bool shouldEmit = isMoving || (Time.time - lastMoveTime <= tailDuration);

        // 3. 마우스 위치로 오브젝트 이동
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(currentMousePos.x, currentMousePos.y, zDistanceFromCamera)
        );
        transform.position = worldPos;

        // 4. Emission 켜고/끄기
        ParticleSystem.EmissionModule emission = trailVFX.emission;
        if (emission.enabled != shouldEmit)
        {
            emission.enabled = shouldEmit;
        }

        lastMousePosition = currentMousePos;
    }
}

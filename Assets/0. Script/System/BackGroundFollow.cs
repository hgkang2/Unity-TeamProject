using UnityEngine;

public class BackGroundFollow : MonoBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField, Range(0f, 1f)] float followFactor = 0.15f;

    Vector3 startPos;
    float camStartX;

    void Awake()
    {
        if (cam == null)
        {
            Camera main = Camera.main;
            if (main != null) cam = main.transform;
        }

        startPos = transform.position;
        camStartX = cam != null ? cam.position.x : 0f;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float camDeltaX = cam.position.x - camStartX;
        float x = startPos.x + camDeltaX * followFactor;

        // Y는 startPos.y 고정
        transform.position = new Vector3(x, startPos.y, startPos.z);
    }
}

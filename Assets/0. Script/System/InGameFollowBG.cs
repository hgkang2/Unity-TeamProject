using UnityEngine;

public class InGameFollowBG : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] float followFactor = 0.15f;

    Transform cam;
    Vector3 startPos;
    float camStartX;

    void Awake()
    {
        CacheCamera();
        ResetOrigin();
    }

    void OnEnable()
    {
        CacheCamera();
        ResetOrigin();
    }

    void CacheCamera()
    {
        Camera main = Camera.main;
        cam = main != null ? main.transform : null;
    }

    public void ResetOrigin()
    {
        startPos = transform.position;
        camStartX = cam != null ? cam.position.x : 0f;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float camDeltaX = cam.position.x - camStartX;
        float x = startPos.x + camDeltaX * followFactor;

        transform.position = new Vector3(x, startPos.y, startPos.z);
    }
}

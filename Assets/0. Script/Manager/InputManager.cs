using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] VFXManager vfxManager;
    [SerializeField] Camera mainCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            clickPos.z = 0f;

            vfxManager.PlayClickEffect(clickPos);
        }
    }
}

using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [Header("Refs")]
    RectTransform canvasRect;
    RectTransform uiRect;
    Canvas canvas;

    CanvasGroup cg;
    Interactor interactor;

    Transform followTarget;

    void Awake()
    {
        uiRect = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();

        SceneContext sc = FindFirstObjectByType<SceneContext>();
        canvas = sc.canvas;
        canvasRect = sc.canvas.GetComponent<RectTransform>();
        interactor = sc.interactor;

        interactor.SelectedChanged += HandleSelectedChanged;

        Hide(); // 시작은 숨김
    }

    void OnDestroy()
    {
        if (interactor != null)
            interactor.SelectedChanged -= HandleSelectedChanged;
    }

    void LateUpdate()
    {
        if (followTarget == null) return;
        UpdatePosition();
    }

    void HandleSelectedChanged(IInteractable target)
    {
        if (target == null)
        {
            followTarget = null;
            Hide();
            return;
        }

        followTarget = target.InteractionUIPosition;
        Show();
        UpdatePosition(); // 즉시 1회 갱신
    }

    void UpdatePosition()
    {
        // 카메라 뒤면 숨김(선택적으로)
        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToScreenPoint(followTarget.position);
        if (screenPos.z < 0f)
        {
            Hide();
            return;
        }

        Camera uiCam =
            canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiCam,
            out Vector2 localPos
        );

        uiRect.localPosition = localPos;
    }

    void Show()
    {
        cg.alpha = 1f;
        cg.blocksRaycasts = false;   // 프롬프트면 보통 false
        cg.interactable = false;
    }

    void Hide()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }
}

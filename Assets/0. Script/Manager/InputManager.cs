using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    Camera mainCamera;

    GameInputActions input;

    void Awake()
    {
        // 싱글톤
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        input = new GameInputActions();
    }

    void OnEnable()
    {
        if (input == null) return;
        input.Enable();
        Subscribe();
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshCameras();
    }

    void OnDisable()
    {
        if (input == null) return;
        UnSubscribe();
        input.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 바뀔 때마다 새 카메라들 다시 물어오기
        RefreshCameras();
    }

    void RefreshCameras()
    {
        if (mainCamera == null || !mainCamera)
            mainCamera = Camera.main;
    }


    #region Player
    public Vector2 Move { get; private set; }
    void OnMove(InputAction.CallbackContext ctx)
    {
        Move = ctx.ReadValue<Vector2>();
    }

    void OnMoveCancel(InputAction.CallbackContext ctx)
    {
        Move = Vector2.zero;
    }

    public bool IsJumpHeld => input.Player.Jump.IsPressed();
    public event Action JumpPressed;
    void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        JumpPressed?.Invoke();
    }

    public event Action DodgePressed;
    void OnDodgePerformed(InputAction.CallbackContext ctx)
    {
        DodgePressed?.Invoke();
    }

    public event Action InteractPressed;
    void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        InteractPressed?.Invoke();
    }
    #endregion

    #region PlayerAtk
    public event Action AttackPressed;
    void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        AttackPressed?.Invoke();
    }

    public event Action SpecialAttackPressed;
    void OnSpecialPerformed(InputAction.CallbackContext ctx)
    {
        SpecialAttackPressed?.Invoke();
    }
    #endregion

    #region UI Input
    public event Action<Vector2> UINavigateStarted;
    void OnUINavigatePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 value = ctx.ReadValue<Vector2>();
        Vector2 dir = NormalizeUINav(value);

        if (dir == Vector2.zero)
            return;

        UINavigateStarted?.Invoke(dir);
    }

    public event Action UINavigateCanceled;
    void OnUINavigateCanceled(InputAction.CallbackContext ctx)
    {
        UINavigateCanceled?.Invoke();
    }

    Vector2 NormalizeUINav(Vector2 value)
    {
        if (value.sqrMagnitude < 0.01f)
            return Vector2.zero;

        if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
            return new Vector2(Mathf.Sign(value.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(value.y));
    }

    // --- 확인 / 취소 ---
    public event Action UICanceled;
    void OnUICancelPerformed(InputAction.CallbackContext ctx)
    {
        UICanceled?.Invoke();
    }
    public event Action UIConfirmed;
    void OnUIConfirmPerformed(InputAction.CallbackContext ctx)
    {
        UIConfirmed?.Invoke();
    }
    public event Action UiRerolled;
    void OnUIRerollPerformed(InputAction.CallbackContext ctx)
    {
        UiRerolled?.Invoke();
    }
    #endregion

    #region System Input
    void OnLeftClick(InputAction.CallbackContext ctx)
    {
        VFXManager.Instance.MouseClickVFX();
        //SoundManager.Instance.PlayUI("Click");
    }
    #endregion

    //기타
    public Vector3 GetMouseOriginPos()
    {
        if (Mouse.current == null)
            return Vector3.zero;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        return new Vector3(screenPos.x, screenPos.y, 0);
    }

    public Vector3 GetMouseWorldPos()
    {
        Vector2 screenPos = input.System.MousePos.ReadValue<Vector2>();

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane)
        );

        // 2D 게임이면 z를 0으로 고정
        worldPos.z = 0f;
        return worldPos;
    }

    #region  이벤트 구독
    void Subscribe()
    {
        if (input == null) return;
        UnSubscribe();

        //Player 입력 이벤트
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMoveCancel;
        input.Player.Jump.performed += OnJumpPerformed;
        input.Player.Dodge.performed += OnDodgePerformed;
        input.Player.Interact.performed += OnInteractPerformed;

        //Player Attack 입력 이벤트
        input.Player.Attack.performed += OnAttackPerformed;
        input.Player.SpecialAttack.performed += OnSpecialPerformed;

        //UI 키보드 입력 이벤트
        input.UI.Navigate.performed += OnUINavigatePerformed;
        input.UI.Navigate.canceled += OnUINavigateCanceled;
        input.UI.Confirm.performed += OnUIConfirmPerformed;
        input.UI.Cancel.performed += OnUICancelPerformed;
        input.UI.Reroll.performed += OnUIRerollPerformed;

        //기타 시스템 이벤트
        input.System.LeftClick.performed += OnLeftClick;
    }
    void UnSubscribe()
    {
        if (input == null) return;

        input.System.LeftClick.performed -= OnLeftClick;

        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMoveCancel;
        input.Player.Jump.performed -= OnJumpPerformed;
        input.Player.Dodge.performed -= OnDodgePerformed;
        input.Player.Interact.performed -= OnInteractPerformed;

        input.UI.Navigate.performed -= OnUINavigatePerformed;
        input.UI.Navigate.canceled -= OnUINavigateCanceled;
        input.UI.Confirm.performed -= OnUIConfirmPerformed;
        input.UI.Cancel.performed -= OnUICancelPerformed;
        input.UI.Reroll.performed -= OnUIRerollPerformed;

        input.Player.Attack.performed -= OnAttackPerformed;
        input.Player.SpecialAttack.performed -= OnSpecialPerformed;
    }
    #endregion
}

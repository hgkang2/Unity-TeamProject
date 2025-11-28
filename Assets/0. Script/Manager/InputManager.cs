using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    static InputManager inputManager;
    public static InputManager Instance => inputManager;
    [SerializeField] VFXManager vfxManager;
    [SerializeField] Camera mainCamera;

    GameInputActions input;





    void Awake()
    {
        Debug.Log($"inputmanager awake");
        // 싱글톤
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        inputManager = this;
        DontDestroyOnLoad(gameObject);

        input = new GameInputActions();
    }

    void OnEnable()
    {
        input.Enable();


        //Player Move 입력 이벤트
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMoveCancel;
        input.Player.Jump.performed += OnJumpPerformed;
        input.Player.Dodge.performed += OnDodgePerformed;

        //Player Attack 입력 이벤트
        input.Player.Attack.performed += OnAttackPerformed;
        input.Player.SpecialAttack.performed += OnSpecialPerformed;

        //UI 키보드 입력 이벤트
        input.UI.Navigate.performed += OnUINavigatePerformed;
        input.UI.Navigate.canceled += OnUINavigateCanceled;
        input.UI.Confirm.performed += OnUIConfirmPerformed;
        input.UI.Cancel.performed += OnUICancelPerformed;

        //기타 시스템 이벤트
        input.System.LeftClick.performed += OnLeftClick;
    }

    void OnDisable()
    {
        input.System.LeftClick.performed -= OnLeftClick;

        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMoveCancel;
        input.Player.Jump.performed -= OnJumpPerformed;
        input.Player.Dodge.performed -= OnDodgePerformed;

        input.UI.Navigate.performed -= OnUINavigatePerformed;
        input.UI.Navigate.canceled -= OnUINavigateCanceled;
        input.UI.Confirm.performed -= OnUIConfirmPerformed;
        input.UI.Cancel.performed -= OnUICancelPerformed;

        input.Player.Attack.performed -= OnAttackPerformed;
        input.Player.SpecialAttack.performed -= OnSpecialPerformed;
        input.Disable();
    }

    void Update()
    {
    }



    #region PlayerMove
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

        if (UINavigateStarted != null)
        {
            UINavigateStarted(dir);
        }
    }

    public event Action UINavigateCanceled;
    void OnUINavigateCanceled(InputAction.CallbackContext ctx)
    {
        if (UINavigateCanceled != null)
        {
            UINavigateCanceled();
        }
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
        if (UICanceled != null)
        {
            UICanceled.Invoke();
        }
    }
    public event Action UIConfirmed;
    void OnUIConfirmPerformed(InputAction.CallbackContext ctx)
    {
        if (UIConfirmed != null)
        {
            UIConfirmed.Invoke();
        }
    }

    #endregion

    #region System Input
    void OnLeftClick(InputAction.CallbackContext ctx)
    {
        vfxManager.MouseClickVFX();
    }
    #endregion

    //기타
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
}

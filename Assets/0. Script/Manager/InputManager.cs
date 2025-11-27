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
    public Vector2 Move { get; private set; }
    public bool IsJumpHeld
    {
        get { return input.Player.Jump.IsPressed(); }
    }

    public event Action AttackPressed;
    public event Action SpecialAttackPressed;
    public event Action JumpPressed;
    public event Action DodgePressed;




    void Awake()
    {
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

        input.System.LeftClick.performed += OnLeftClick;

        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMoveCancel;
        input.Player.Jump.performed += OnJumpPerformed;
        input.Player.Dodge.performed += OnDodgePerformed;

        input.Player.Attack.performed += OnAttackPerformed;
        input.Player.SpecialAttack.performed += OnSpecialPerformed;
    }

    void OnDisable()
    {
        input.System.LeftClick.performed -= OnLeftClick;

        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMoveCancel;
        input.Player.Jump.performed -= OnJumpPerformed;
        input.Player.Dodge.performed -= OnDodgePerformed;

        input.Player.Attack.performed -= OnAttackPerformed;
        input.Player.SpecialAttack.performed -= OnSpecialPerformed;
        input.Disable();
    }

    void Update()
    {
    }
    #region System Input
    void OnLeftClick(InputAction.CallbackContext ctx)
    {
        vfxManager.MouseClickVFX();
    }
    #endregion


    #region PlayerMove Input
    void OnMove(InputAction.CallbackContext ctx)
    {
        Move = ctx.ReadValue<Vector2>();
    }

    void OnMoveCancel(InputAction.CallbackContext ctx)
    {
        Move = Vector2.zero;
    }

    void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        JumpPressed?.Invoke();
    }

    void OnDodgePerformed(InputAction.CallbackContext ctx)
    {
        DodgePressed?.Invoke();
    }
    #endregion

    #region  PlayerAtk Input
    void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        AttackPressed?.Invoke();
    }

    void OnSpecialPerformed(InputAction.CallbackContext ctx)
    {
        SpecialAttackPressed?.Invoke();
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

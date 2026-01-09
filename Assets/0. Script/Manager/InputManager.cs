using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    Camera mainCamera;

    GameInputActions input;


    [Header("UI Navigate Repeat")]
    [SerializeField] float uiInitialDelay = 0.4f;
    [SerializeField] float uiRepeatInterval = 0.08f;

    Vector2 uiHeldDir;
    bool uiIsHeld;
    float uiNextRepeatTime;



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
        input.System.Enable();
        ApplyInputMode();
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

    void Update()
    {
        if (!uiIsHeld || uiHeldDir == Vector2.zero)
            return;

        if (Time.unscaledTime >= uiNextRepeatTime)
        {
            TopUI?.OnUIMove(uiHeldDir);
            uiNextRepeatTime = Time.unscaledTime + uiRepeatInterval;
        }
    }

    void RefreshCameras()
    {
        if (mainCamera == null || !mainCamera)
            mainCamera = Camera.main;
    }

    #region UI 입력 제어
    // 모든 키보드 입력을 받는 UI
    readonly Stack<IUIKeyboardTarget> uiTargets = new();
    // UI 중에 플레이어 입력을 막는 UI
    int modalCount = 0; // blocksGameplay=true UI가 하나라도 열려있으면 > 0

    IUIKeyboardTarget TopUI => uiTargets.Count > 0 ? uiTargets.Peek() : null;


    void ApplyInputMode()
    {
        bool uiModalActive = modalCount > 0;
        
        uiIsHeld = false;
        uiHeldDir = Vector2.zero;

        if (uiModalActive)
        {
            // UI 모달이 하나라도 있으면 플레이어 입력 강제 차단
            Move = Vector2.zero;

            input.Player.Disable();
            input.UI.Enable();
        }
        else
        {
            input.UI.Disable();
            input.Player.Enable();
        }


        input.System.Enable();
    }
    void ResetUINavHold()
    {
        
    }

    public void PushUI(IUIKeyboardTarget target, bool blocksGameplay)
    {
        uiTargets.Push(target);

        if (blocksGameplay)
            modalCount++;

        ApplyInputMode();
    }

    public void PopUI(IUIKeyboardTarget target, bool blocksGameplay)
    {
        // top 규칙 강제
        if (uiTargets.Count == 0 || !ReferenceEquals(uiTargets.Peek(), target))
            throw new System.Exception("[InputManager] PopUI called by non-top target");

        uiTargets.Pop();

        if (blocksGameplay)
            modalCount--;


        if (modalCount < 0)
            throw new System.Exception("[InputManager] modalCount < 0 (Pop mismatch)");

        ApplyInputMode();
    }
    #endregion


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
    void OnUINavigatePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 value = ctx.ReadValue<Vector2>();
        Vector2 dir = NormalizeUINav(value);

        if (dir == Vector2.zero) return;

        // 방향 저장 + 홀드 시작
        uiHeldDir = dir;
        uiIsHeld = true;

        // 첫 입력은 즉시 1회
        TopUI?.OnUIMove(dir);

        // 이후 반복 예약 (unscaled)
        uiNextRepeatTime = Time.unscaledTime + uiInitialDelay;
    }
    void OnUINavigateCanceled(InputAction.CallbackContext ctx)
    {
        uiIsHeld = false;
        uiHeldDir = Vector2.zero;
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
    void OnUICancelPerformed(InputAction.CallbackContext ctx)
    {
        TopUI?.OnUICancel();
    }
    void OnUIConfirmPerformed(InputAction.CallbackContext ctx)
    {
        TopUI?.OnUIConfirm();
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
        input.UI.Confirm.performed -= OnUIConfirmPerformed;
        input.UI.Cancel.performed -= OnUICancelPerformed;
        input.UI.Reroll.performed -= OnUIRerollPerformed;

        input.Player.Attack.performed -= OnAttackPerformed;
        input.Player.SpecialAttack.performed -= OnSpecialPerformed;
    }
    #endregion
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
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

    public IUIKeyboardTarget TopUI => uiTargets.Count > 0 ? uiTargets.Peek() : null;


    void ApplyInputMode()
    {
        bool uiModalActive = modalCount > 0;
        Debug.Log($"[Input] modalCount={modalCount}, TopUI={(TopUI==null?"null":TopUI.ToString())}, uiModalActive={uiModalActive}");
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

    public void RemoveUI(IUIKeyboardTarget target, bool blocksGameplay)
    {
        // 스택에서 target을 찾아 제거 (top 아니어도)
        if (uiTargets.Count == 0) return;

        var tmp = new Stack<IUIKeyboardTarget>();
        bool removed = false;

        while (uiTargets.Count > 0)
        {
            var cur = uiTargets.Pop();
            if (!removed && ReferenceEquals(cur, target))
            {
                removed = true;
                continue;
            }
            tmp.Push(cur);
        }

        while (tmp.Count > 0)
            uiTargets.Push(tmp.Pop());

        if (removed && blocksGameplay)
            modalCount--;

        if (modalCount < 0)
            modalCount = 0; // 또는 예외

        ApplyInputMode();
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
    void OnUINavigatePerformed(InputAction.CallbackContext ctx)
    {
        TopUI?.OnUIInputMove(ctx.ReadValue<Vector2>());
    }
    void OnUINavigateCanceled(InputAction.CallbackContext ctx)
    {
        TopUI?.OnUIInputMove(Vector2.zero);
    }


    // --- 확인 / 취소 ---
    void OnUICancelPerformed(InputAction.CallbackContext ctx)
    {
        TopUI?.OnUIInputCancel();
    }
    void OnUIConfirmPerformed(InputAction.CallbackContext ctx)
    {
        TopUI?.OnUIInputConfirm();
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
    public event Action EscPressed;
    void OnSystemEscPerformed(InputAction.CallbackContext ctx)
    {
        if (TopUI != null) return;

        // TopUI가 없을 때는 “씬 기본 UI(예: StageUI)”가 처리하게 하고 싶으면
        // 이벤트로 빼는 게 가장 안전함 (아래 이벤트 방식 참고)
        EscPressed?.Invoke();
    }

    // Z 홀드: 누를 때
    public event Action ZPressed;
    void OnSystemZPerformed(InputAction.CallbackContext ctx)
    {
        ZPressed?.Invoke();
    }

    // Z 홀드: 뗄 때
    public event Action ZReleased;
    void OnSystemZCanceled(InputAction.CallbackContext ctx)
    {
        ZReleased?.Invoke();
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
        input.System.Esc.performed += OnSystemEscPerformed;
        input.System.Z.performed += OnSystemZPerformed;
        input.System.Z.canceled += OnSystemZCanceled;
    }
    void UnSubscribe()
    {
        if (input == null) return;

        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMoveCancel;
        input.Player.Jump.performed -= OnJumpPerformed;
        input.Player.Dodge.performed -= OnDodgePerformed;
        input.Player.Interact.performed -= OnInteractPerformed;

        input.Player.Attack.performed -= OnAttackPerformed;
        input.Player.SpecialAttack.performed -= OnSpecialPerformed;

        input.UI.Navigate.performed -= OnUINavigatePerformed;
        input.UI.Navigate.canceled -= OnUINavigateCanceled;
        input.UI.Confirm.performed -= OnUIConfirmPerformed;
        input.UI.Cancel.performed -= OnUICancelPerformed;
        input.UI.Reroll.performed -= OnUIRerollPerformed;

        input.System.LeftClick.performed -= OnLeftClick;
        input.System.Esc.performed -= OnSystemEscPerformed;
        input.System.Z.performed -= OnSystemZPerformed;
        input.System.Z.canceled -= OnSystemZCanceled;
    }
    #endregion
}

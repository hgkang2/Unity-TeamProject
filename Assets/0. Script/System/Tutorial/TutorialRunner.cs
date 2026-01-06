using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRunner : MonoBehaviour
{
    // 참조들
    DialoguePanel dialoguePanel;
    Player player;
    TargetTrackerEmitter2D targetTrackerEmitter2D; // player 아래에 있음(TurotialCollider)

    // 플레이어가 닿으면 발동되는 트리거
    //[SerializeField] TutorialTrigger2D tutorialTrigger;

    List<ITutorialStep> steps;
    ITutorialStep currentStep;
    int stepIndex;
    bool isRunning;

    void Awake()
    {
        SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
        dialoguePanel = sceneContext.dialoguePanel;
        player = sceneContext.player;
        targetTrackerEmitter2D = sceneContext.targetTrackerEmitter2D;
    }

    void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.UIConfirmed += OnUIConfirm;
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.UIConfirmed -= OnUIConfirm;
    }

    public void StartTutorial()
    {
        BuildSteps();

        stepIndex = 0;
        isRunning = true;

        StartCurrentStep();
    }

    public void StopTutorial()
    {
        isRunning = false;

        if (currentStep != null)
        {
            currentStep.Exit();
            currentStep = null;
        }
    }

    void Update()
    {
        if (!isRunning || currentStep == null)
            return;

        currentStep.Tick();

        if (!currentStep.IsDone)
            return;

        currentStep.Exit();
        stepIndex++;

        if (steps == null || stepIndex >= steps.Count)
        {
            isRunning = false;
            currentStep = null;
            return;
        }

        StartCurrentStep();
    }

    void StartCurrentStep()
    {
        currentStep = steps[stepIndex];
        currentStep.Enter();
    }

    void OnUIConfirm()
    {
        // 대화 중이면 Confirm은 대화가 우선 소비
        if (dialoguePanel != null && dialoguePanel.IsPlaying)
            dialoguePanel.Confirm();
    }

    // -----------------------
    // Step 구성
    // -----------------------


    void BuildSteps()
    {
        steps = new List<ITutorialStep>();

        // 1) 시작 대사
        steps.Add(new CallStep(() => { player.SetControlLocked(true); }));
        steps.Add(new DialogueStep(dialoguePanel, new string[]
        {
            "으으으… 기분 나쁜 숲이야…",
            "검은.. 여명회… 그들은 대체 왜 이런짓을…"
        }));
        steps.Add(new CallStep(() => { player.SetControlLocked(false); }));
        // 2) 몬스터 발견
        steps.Add(new WaitEventCountStep(
            subscribe: cb => targetTrackerEmitter2D.TargetEntered += cb,
            unsubscribe: cb => targetTrackerEmitter2D.TargetEntered -= cb,
            targetCount: 1
        ));

        // 2) 멈추고 공격 키 누르게 하기

        steps.Add(new CallStep(() => { player.SetControlLocked(true); }));
        steps.Add(new CallStep(() => { TimeManager.Pause(); }));
        steps.Add(new DialogueStep(dialoguePanel, new string[]
        {
            "괴물이잖아!!  [A] 키를 입력해서 공격 하자!"
        }));

        steps.Add(new CallStep(() => { player.SetControlLocked(false); }));
        steps.Add(new WaitEventCountStep(
            SubscribeAttackCommitted,
            UnsubscribeAttackCommitted, 1
        ));
        steps.Add(new CallStep(() => { TimeManager.Resume(); }));

        steps.Add(new WaitSecondsStep(1.5f));

        // 3) 점프
        steps.Add(new CallStep(() => { player.SetControlLocked(true); }));
        steps.Add(new CallStep(() => { TimeManager.Pause(); }));
        steps.Add(new DialogueStep(dialoguePanel, new string[]
        {
            "저 녀석이 무언가 하려고 한다! 점프해서 피하자."
        }));
        steps.Add(new CallStep(() => { player.SetControlLocked(false); }));
        steps.Add(new WaitEventCountStep(SubscribeJump, UnsubscribeJump, 1));
        steps.Add(new CallStep(() => { TimeManager.Resume(); }));

        steps.Add(new WaitSecondsStep(1f));

        // 4) 아래공격
        steps.Add(new CallStep(() => { player.SetControlLocked(true); }));
        steps.Add(new CallStep(() => TimeManager.Pause()));
        steps.Add(new DialogueStep(dialoguePanel, new string[]
        {
            "지금이야!! [↓] + [A] 키 입력으로 하단 공격을 하자!"
        }));
        steps.Add(new CallStep(() => { player.SetControlLocked(false); }));
        steps.Add(new WaitEventCountStep(
            SubscribeAttackCommitted,
            UnsubscribeAttackCommitted, 1
            ));
        steps.Add(new CallStep(() => TimeManager.Resume()));

        steps.Add(new DialogueStep(dialoguePanel, new string[]
        {
            "완료!"
        }));
        steps.Add(new CallStep(OnTutorialFinished));
    }

    void SubscribeAttackCommitted(Action callback)
    {
        InputManager.Instance.AttackPressed += callback;
    }

    void UnsubscribeAttackCommitted(Action callback)
    {
        InputManager.Instance.AttackPressed -= callback;
    }

    void SubscribeJump(Action callback)
    {
        InputManager.Instance.JumpPressed += callback;
    }

    void UnsubscribeJump(Action callback)
    {
        InputManager.Instance.JumpPressed -= callback;
    }

    void OnTutorialFinished()
    {
        // 예: 다음 씬
        // SceneLoader.LoadScene("Stage1");
    }


    #region Tutorial Step Interfaces / Implementations

    public class TutorialTrigger2D : MonoBehaviour
    {
        public event Action Triggered;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            Triggered?.Invoke();
        }
    }

    public interface ITutorialStep
    {
        void Enter();
        void Tick();
        bool IsDone { get; }
        void Exit();
    }


    // 1) Enter에서 1회 실행하고 바로 끝나는 Step (Pause/Resume, 게이트 on/off 등)
    public sealed class CallStep : ITutorialStep
    {
        readonly Action onEnter;
        readonly Action onExit;

        public bool IsDone { get; private set; }

        public CallStep(Action onEnter, Action onExit = null)
        {
            this.onEnter = onEnter;
            this.onExit = onExit;
        }

        public void Enter()
        {
            IsDone = false;
            onEnter?.Invoke();
            IsDone = true;
        }

        public void Tick() { }

        public void Exit()
        {
            onExit?.Invoke();
        }
    }

    // 2) 언스케일 시간으로 N초 대기
    public sealed class WaitSecondsStep : ITutorialStep
    {
        readonly float seconds;
        float elapsed;

        public bool IsDone { get; private set; }

        public WaitSecondsStep(float seconds)
        {
            this.seconds = seconds;
        }

        public void Enter()
        {
            elapsed = 0f;
            IsDone = false;
        }

        public void Tick()
        {
            if (IsDone) return;

            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= seconds)
                IsDone = true;
        }

        public void Exit() { }
    }

    // 3) dialoguePanel로 대사를 재생하고, 끝나면 완료
    public sealed class DialogueStep : ITutorialStep
    {
        readonly DialoguePanel dialoguePanel;
        readonly DialogueAsset asset;
        readonly string[] lines;

        public bool IsDone { get; private set; }

        // SO 버전
        public DialogueStep(DialoguePanel player, DialogueAsset asset)
        {
            this.dialoguePanel = player;
            this.asset = asset;
            lines = null;
        }

        // 하드코딩 버전
        public DialogueStep(DialoguePanel player, string[] lines)
        {
            this.dialoguePanel = player;
            this.lines = lines;
            asset = null;
        }

        public void Enter()
        {
            IsDone = false;

            if (asset != null)
            {
                dialoguePanel.Play(asset, OnCompleted);
                return;
            }

            dialoguePanel.Play(lines, OnCompleted);
        }

        void OnCompleted()
        {
            IsDone = true;
        }

        public void Tick() { }

        public void Exit() { }
    }

    // 4) 특정 이벤트(입력/행동) N회 발생할 때까지 대기
    public sealed class WaitEventCountStep : ITutorialStep
    {
        readonly Action<Action> subscribe;
        readonly Action<Action> unsubscribe;
        readonly int targetCount;

        int count;
        public bool IsDone { get; private set; }

        public WaitEventCountStep(
            Action<Action> subscribe,
            Action<Action> unsubscribe,
            int targetCount = 1)
        {
            this.subscribe = subscribe;
            this.unsubscribe = unsubscribe;
            this.targetCount = Mathf.Max(1, targetCount);
        }

        public void Enter()
        {
            IsDone = false;
            count = 0;
            subscribe?.Invoke(OnEvent);
        }

        void OnEvent()
        {
            if (IsDone) return;

            count++;
            if (count >= targetCount)
                IsDone = true;
        }

        public void Tick() { }

        public void Exit()
        {
            unsubscribe?.Invoke(OnEvent);
        }
    }

    // 조건 넣기
    public sealed class WaitEventCountStep<T> : ITutorialStep
    {
        readonly Action<Action<T>> subscribe;
        readonly Action<Action<T>> unsubscribe;
        readonly Func<T, bool> accept;   // 조건 필터 (null이면 전부 통과)
        readonly int targetCount;

        int count;

        public bool IsDone { get; private set; }

        public WaitEventCountStep(
            Action<Action<T>> subscribe,
            Action<Action<T>> unsubscribe,
            Func<T, bool> accept = null,
            int targetCount = 1)
        {
            this.subscribe = subscribe;
            this.unsubscribe = unsubscribe;
            this.accept = accept;
            this.targetCount = Mathf.Max(1, targetCount);
        }

        public void Enter()
        {
            IsDone = false;
            count = 0;

            subscribe?.Invoke(OnEvent);
        }

        void OnEvent(T value)
        {
            if (IsDone) return;

            if (accept != null && !accept(value))
                return;

            count++;

            if (count >= targetCount)
                IsDone = true;
        }

        public void Tick() { }

        public void Exit()
        {
            unsubscribe?.Invoke(OnEvent);
        }
    }


    #endregion

}

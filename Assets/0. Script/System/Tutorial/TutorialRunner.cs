using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//GameManager에 붙여놓음
public class TutorialRunner : MonoBehaviour
{
    // 참조들
    [SerializeField] EventHub eventHub;
    DialoguePanel dialoguePanel;
    Player player;
    // 처음 몬스터 만났을 때 대사 띄우는 트리거
    [SerializeField] TutorialTrigger2D tutorialTrigger_monsterMeet; 
    // 몹 처치하기 전까지 다음으로 못 가게 막는 벽
    [SerializeField] Collider2D tutorialWall;
    // 함정 앞에서 대사 띄우는 트리거
    [SerializeField] TutorialTrigger2D tutorialTrigger_trap;

    List<ITutorialStep> steps;
    ITutorialStep currentStep;
    int stepIndex;
    bool isRunning;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Stage1":
            case "Stage1_Test":
                SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
                dialoguePanel = sceneContext.dialoguePanel;
                dialoguePanel.gameObject.SetActive(true);
                player = sceneContext.player;
                break;
        }

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
        // 테스트용. 나중에 꼭 삭제하기
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            eventHub.monster.RaiseMonsterDead();
        }

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
            dialoguePanel.OnUIInputConfirm();
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

        // 2) 몬스터 발견시 일반 공격 안내
        steps.Add(new WaitEventCountStep(
            subscribe: cb => tutorialTrigger_monsterMeet.Triggered += cb,
            unsubscribe: cb => tutorialTrigger_monsterMeet.Triggered -= cb,
            targetCount: 1
        ));
       
        steps.Add(new CallStep(() => { TimeManager.Pause(); }));
        steps.Add(new DialogueStep(dialoguePanel, new string[]
        {
            "괴물이잖아!!  [A] 키를 입력해서 공격하자!",
            "저 녀석이 공격하려고 하면 점프[Space] 혹은 회피[D] 로 피하자."
        }));
        steps.Add(new CallStep(() => { TimeManager.Resume(); }));

        // 5) 몹 처치까지 대기
        steps.Add(new WaitEventCountStep(
            cb => eventHub.monster.monsterDead += cb, 
            cb => eventHub.monster.monsterDead -= cb, 1
        ));

        steps.Add(new WaitSecondsStep(1.5f));
        
        // 6) 몹 처치 완료 후 종료
        steps.Add(new CallStep(() => { player.SetControlLocked(true); }));
        steps.Add(new DialogueStep(dialoguePanel, new string[]
        {
            "후.. 앞으로 이런 괴물들을 상대해야 하는건가??",
            "조금.. 버거울지도 모르겠어.. 일단 앞으로 가보자."
        }));
        steps.Add(new CallStep(() => { player.SetControlLocked(false); }));
        steps.Add(new CallStep(() => { tutorialWall.gameObject.SetActive(false); }));

        // 7) 함정 앞에서 경고
        steps.Add(new WaitEventCountStep(
            subscribe: cb => tutorialTrigger_trap.Triggered += cb,
            unsubscribe: cb => tutorialTrigger_trap.Triggered -= cb,
            targetCount: 1
        ));
        steps.Add(new CallStep(() => { player.SetControlLocked(true); }));
        steps.Add(new DialogueStep(dialoguePanel, new string[]
        {
            "조심해!! 앞에 함정이 있어!"
        }));
        steps.Add(new CallStep(() => { CameraManager.Instance.ChangeCinemachineTutorialTrap(); }));
        steps.Add(new WaitEventCountStep(
            subscribe: cb => CameraManager.Instance.CinemachineSequenceFinished += cb,
            unsubscribe: cb => CameraManager.Instance.CinemachineSequenceFinished -= cb,
            targetCount: 1
        ));
        steps.Add(new CallStep(() => { player.SetControlLocked(false); }));

        steps.Add(new CallStep(OnTutorialFinished));
    }

    void SubscribeDodgePressed(Action callback)
    {
        InputManager.Instance.JumpPressed += callback;
        InputManager.Instance.DodgePressed += callback;
    }
    void UnsubscribeDodgePressed(Action callback)
    {
        InputManager.Instance.JumpPressed -= callback;
        InputManager.Instance.DodgePressed -= callback;
    }


    void OnTutorialFinished()
    {
        // 예: 다음 씬
        // SceneLoader.LoadScene("Stage1");
    }


    #region Tutorial Step Interfaces / Implementations
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

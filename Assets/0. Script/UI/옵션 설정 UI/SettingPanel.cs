using System.Collections.Generic;
using UnityEngine;

public class SettingPanel : UIPanelBase
{
    enum NavAxis { None, X, Y }

    [Header("Setting Items (Top → Bottom)")]
    [SerializeField] List<MonoBehaviour> settingItemBehaviours;

    [SerializeField] SaveAlertPanel saveAlertPanel_GotoTitle;
    [SerializeField] SaveAlertPanel saveAlertPanel_QuitGame;

    List<ISettingItem> items = new();
    Dictionary<ISettingItem, int> indexByItem = new();
    int currentIndex;

    // ---- Hold state ----
    int holdX;                 // -1 / +1 / 0
    float holdElapsed;
    float nextFireTime;
    float accelInterval;
    ISettingItem holdItem;

    public void OpenSaveAlertPanel_GotoTitle() => saveAlertPanel_GotoTitle.Open();
    public void OpenSaveAlertPanel_QuitGame() => saveAlertPanel_QuitGame.Open();

    protected override void Init()
    {
        items.Clear();
        indexByItem.Clear();

        foreach (var mb in settingItemBehaviours)
        {
            if (mb is ISettingItem item)
                items.Add(item);
        }
        for (int i = 0; i < items.Count; i++)
            indexByItem[items[i]] = i;

        currentIndex = Mathf.Clamp(currentIndex, 0, items.Count - 1);

        currentIndex = 4;

        RefreshSelection();

        saveAlertPanel_GotoTitle.gameObject.SetActive(true);
        saveAlertPanel_QuitGame.gameObject.SetActive(true);

        saveAlertPanel_GotoTitle.Close();
        saveAlertPanel_QuitGame.Close();
    }

    void Update()
    {
        if (holdX == 0) return;

        // 패널이 열려있을 때 UI는 보통 timescale 0이어도 돌아가야 하니까 unscaled 사용
        holdElapsed += Time.unscaledDeltaTime;

        if (holdElapsed < nextFireTime)
            return;

        // 다음 반복 실행
        FireAdjustOnce();
        ScheduleNext();
    }



    NavAxis activeAxis = NavAxis.None;
    Vector2 lastRaw;

    public override void OnUIInputMove(Vector2 raw)
    {
        // 해제 신호
        if (raw == Vector2.zero)
        {
            lastRaw = Vector2.zero;
            activeAxis = NavAxis.None;
            StopHold();
            return;
        }

        const float TH = 0.1f;

        bool xDown = Mathf.Abs(raw.x) > TH;
        bool yDown = Mathf.Abs(raw.y) > TH;

        bool xWasDown = Mathf.Abs(lastRaw.x) > TH;
        bool yWasDown = Mathf.Abs(lastRaw.y) > TH;

        bool xNewDown = xDown && !xWasDown; // 0 -> 눌림
        bool yNewDown = yDown && !yWasDown;

        int xSign = xDown ? (int)Mathf.Sign(raw.x) : 0;
        int ySign = yDown ? (int)Mathf.Sign(raw.y) : 0;

        int lastXSign = xWasDown ? (int)Mathf.Sign(lastRaw.x) : 0;
        int lastYSign = yWasDown ? (int)Mathf.Sign(lastRaw.y) : 0;

        bool xSignChanged = xDown && xWasDown && (xSign != lastXSign);
        bool ySignChanged = yDown && yWasDown && (ySign != lastYSign);

        // ---- activeAxis 결정 규칙 (디지털 입력용 “마지막 축 우선”) ----
        // 1) 새로 눌린 축이 있으면 그 축으로 전환
        // 2) 같은 축에서 방향이 바뀌면(좌->우) 그 축 유지
        if (xNewDown) activeAxis = NavAxis.X;
        else if (yNewDown) activeAxis = NavAxis.Y;
        else if (xSignChanged && activeAxis == NavAxis.X) { } // 유지
        else if (ySignChanged && activeAxis == NavAxis.Y) { } // 유지
        else if (activeAxis == NavAxis.None)
        {
            // 처음 들어온 경우: X가 있으면 X, 아니면 Y
            if (xDown) activeAxis = NavAxis.X;
            else if (yDown) activeAxis = NavAxis.Y;
        }

        // ---- 축별 처리 ----
        if (activeAxis == NavAxis.Y)
        {
            // 위/아래는 "새로 눌림/방향 전환"일 때만 1칸 이동
            StopHold();

            if (yNewDown || ySignChanged)
            {
                MoveIndex(ySign > 0 ? -1 : +1);
            }
        }
        else if (activeAxis == NavAxis.X)
        {
            // 좌/우는 홀드 엔진 (아이템 repeat 정책에 따라 가속/고정)
            if (!xDown)
            {
                StopHold();
            }
            else if (xNewDown || xSignChanged)
            {
                StartHold(xSign); // 누르는 순간 1회 + 이후 반복은 Update에서
            }
            // xDown 유지 중 반복은 Update()가 처리
        }

        lastRaw = raw;
    }

    public override void OnUIInputConfirm()
    {
        StopHold();

        var cur = items[currentIndex];
        if (cur.CanSubmit)
            cur.Submit();
    }

    void StartHold(int dirX)
    {
        var cur = items[currentIndex];

        // Adjust 불가면 홀드 자체가 의미 없음
        if (!cur.CanAdjust)
        {
            StopHold();
            return;
        }

        // 같은 방향으로 이미 홀드 중이면 무시
        if (holdX == dirX && holdItem == cur)
            return;

        holdX = dirX;
        holdItem = cur;

        // 누르는 순간 즉시 1회 적용
        FireAdjustOnce();

        // 스케줄 초기화
        holdElapsed = 0f;

        if (cur.RepeatMode == UIRepeatMode.FixedInterval)
        {
            nextFireTime = cur.RepeatInterval; // 예: 0.25
        }
        else if (cur.RepeatMode == UIRepeatMode.Accelerate)
        {
            nextFireTime = cur.AccelStartDelay;       // 예: 0.35
            accelInterval = cur.AccelInitialInterval; // 예: 0.25
        }
        else
        {
            // None이면 홀드 반복 없음 (단발로 끝)
            StopHold();
        }
    }

    void FireAdjustOnce()
    {
        holdItem.Adjust(holdX);
    }

    void ScheduleNext()
    {
        holdElapsed = 0f;

        if (holdItem.RepeatMode == UIRepeatMode.FixedInterval)
        {
            nextFireTime = holdItem.RepeatInterval;
            return;
        }

        if (holdItem.RepeatMode == UIRepeatMode.Accelerate)
        {
            // 가속: interval을 점점 줄여서 더 빨라지게
            accelInterval = Mathf.Max(holdItem.AccelMinInterval, accelInterval * holdItem.AccelFactor);
            nextFireTime = accelInterval;
        }
    }

    void StopHold()
    {
        holdX = 0;
        holdElapsed = 0f;
        nextFireTime = 0f;
        accelInterval = 0f;
        holdItem = null;
    }

    void MoveIndex(int delta)
    {
        items[currentIndex].SetSelected(false);

        currentIndex = (currentIndex + delta + items.Count) % items.Count;
        items[currentIndex].SetSelected(true);
    }

    void RefreshSelection()
    {
        for (int i = 0; i < items.Count; i++)
            items[i].SetSelected(i == currentIndex);
    }


    public void RequestFocus(ISettingItem item)
    {
        if (item == null)
            return;

        if (!indexByItem.TryGetValue(item, out int idx))
            return;

        if (idx == currentIndex)
            return;

        StopHold();
        activeAxis = NavAxis.None;
        lastRaw = Vector2.zero;

        if (currentIndex >= 0 && currentIndex < items.Count)
            items[currentIndex].SetSelected(false);

        currentIndex = idx;
        items[currentIndex].SetSelected(true);
    }

    protected override void OnClosing()
    {
    }

    public void GotoTitle()
    {
        SceneLoader.NoLoadingScene("Start");
    }
    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}

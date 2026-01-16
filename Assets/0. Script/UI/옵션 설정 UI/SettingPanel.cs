using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// SettingPanel (SSOT)
/// - 키보드/마우스 동시 조작을 자연스럽게 만들기 위해,
///   포커스(currentIndex)와 하이라이트는 이 패널에서만 관리한다.
/// - 방향키 입력은 디지털 입력 기준 "마지막 축 우선(activeAxis)"으로 해석한다.
///   - Y축: 포커스 이동(네비게이션 홀드 가능)
///   - X축: (1) 좌/우 네비가 존재하면 포커스 이동
///          (2) 아니면 CanAdjust 항목에 한해 값 조절(홀드/가속)
/// - 슬라이더 드래그 중에는 focusLocked로 키보드 입력/포커스 변경을 막는다.
/// </summary>
public class SettingPanel : UIPanelBase, IPointerExitHandler
{
    public enum SettingPanelMode { Ingame, MainMenu }

    [Header("Mode")]
    [SerializeField] SettingPanelMode mode = SettingPanelMode.Ingame;

    #region Types
    enum NavAxis { None, X, Y }

    // navMap 전용: 현재 인덱스에서 Up/Down/Left/Right로 어디로 이동할지
    protected struct Nav4
    {
        public int up, down, left, right;
        public Nav4(int up, int down, int left, int right)
        {
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
        }
    }
    #endregion

    #region Inspector
    [Header("Setting Items (Top → Bottom)")]
    [SerializeField] List<GameObject> settingItemObjects;

    [Header("Confirm Panels")]
    bool isAskingClose;
    [SerializeField] ConfirmPanel alertPanel_SettingSave;
    [SerializeField] ConfirmPanel saveAlertPanel_GotoTitle;
    [SerializeField] ConfirmPanel saveAlertPanel_QuitGame;
    

    [Header("Vertical Hold (Navigation)")]
    [SerializeField] float navHoldStartDelay = 0.2f;
    [SerializeField] float navHoldInterval = 0.2f;
    #endregion

    #region Item Cache
    // 인스펙터 목록에서 ISettingItem만 추려서 캐시
    readonly List<ISettingItem> items = new();
    // 마우스 hover로 들어온 ISettingItem -> index 역매핑
    readonly Dictionary<ISettingItem, int> indexByItem = new();

    // 포커스(SSOT)
    int currentIndex;

    // 그리드/특수 이동을 위한 네비 맵
    protected readonly Dictionary<int, Nav4> navMap = new();
    #endregion

    #region Input State
    // 드래그 중(슬라이더 조작 등)에는 키보드 입력/포커스 변경 잠금
    bool focusLocked;

    // 디지털 입력용 "마지막 축 우선" 판정용 상태
    NavAxis activeAxis = NavAxis.None;
    Vector2 lastRaw;

    const float INPUT_TH = 0.1f;
    #endregion

    #region Hold Engine
    // X: 값 조절(가속/고정) / Y: 네비게이션(고정)
    int holdX; // -1 / +1 / 0
    int holdY; // -1 / +1 / 0

    float holdElapsed;
    float nextFireTime;

    // X 가속용
    float accelInterval;
    ISettingItem holdItem;
    #endregion

    #region Public API
    public void OpenSaveAlertPanel_GotoTitle() => saveAlertPanel_GotoTitle.Open();
    public void OpenSaveAlertPanel_QuitGame() => saveAlertPanel_QuitGame.Open();

    /// <summary>
    /// 슬라이더 드래그 중 포커스/키보드 입력 잠금.
    /// (드래그 시작/끝에서 호출)
    /// </summary>
    public void SetFocusLocked(bool locked)
    {
        focusLocked = locked;

        // 드래그 중에는 홀드/축 상태를 끊어주는 게 안전
        if (locked)
            ResetInputState();
    }

    /// <summary>
    /// 마우스 hover/click → 포커스 변경 요청.
    /// 포커스/하이라이트는 SettingPanel(SSOT)에서만 변경한다.
    /// </summary>
    public void RequestFocus(ISettingItem item)
    {
        if (focusLocked) return;
        if (item == null) return;
        if (!indexByItem.TryGetValue(item, out int idx)) return;
        if (idx == currentIndex) return;

        ResetInputState();

        if (IsValidIndex(currentIndex))
            items[currentIndex].SetSelected(false);

        currentIndex = idx;
        items[currentIndex].SetSelected(true);
    }
    #endregion

    #region UIPanelBase
    protected override void Init()
    {
        BuildItemCache();

        RefreshSelection();
        BuildNavMap();

        // 모달 패널 초기화
        alertPanel_SettingSave?.gameObject.SetActive(true);
        saveAlertPanel_GotoTitle?.gameObject.SetActive(true);
        saveAlertPanel_QuitGame?.gameObject.SetActive(true);
    }

    protected override void OnOpened()
    {
        //기본 선택
        currentIndex = Mathf.Clamp(GetDefaultIndex(), 0, items.Count - 1);
        alertPanel_SettingSave?.Close();
        saveAlertPanel_GotoTitle?.Close();
        saveAlertPanel_QuitGame?.Close();
    }

    protected override void OnClosing()
    {
    }
    #endregion

    #region Update(Hold)
    void Update()
    {
        // 홀드 중인 축이 없으면 스킵
        if (holdX == 0 && holdY == 0) return;

        // UI는 timescale 0에서도 돌아야 하는 경우가 많아서 unscaled 사용
        holdElapsed += Time.unscaledDeltaTime;

        if (holdElapsed < nextFireTime)
            return;

        if (holdX != 0)
        {
            FireAdjustOnce();
            ScheduleAdjustNext();
        }
        else if (holdY != 0)
        {
            FireNavOnce();
            // 다음 네비 반복(고정 간격)
            holdElapsed = 0f;
            nextFireTime = navHoldInterval;
        }
    }
    #endregion

    #region UI Input (kbd)
    public override void OnUIInputMove(Vector2 raw)
    {
        if (focusLocked) return;

        // 해제 신호: 입력 상태/홀드 정리
        if (raw == Vector2.zero)
        {
            ResetInputState();
            return;
        }

        // 입력 상태 파싱
        bool xDown = Mathf.Abs(raw.x) > INPUT_TH;
        bool yDown = Mathf.Abs(raw.y) > INPUT_TH;

        bool xWasDown = Mathf.Abs(lastRaw.x) > INPUT_TH;
        bool yWasDown = Mathf.Abs(lastRaw.y) > INPUT_TH;

        bool xNewDown = xDown && !xWasDown;
        bool yNewDown = yDown && !yWasDown;

        int xSign = xDown ? (int)Mathf.Sign(raw.x) : 0;
        int ySign = yDown ? (int)Mathf.Sign(raw.y) : 0;

        int lastXSign = xWasDown ? (int)Mathf.Sign(lastRaw.x) : 0;
        int lastYSign = yWasDown ? (int)Mathf.Sign(lastRaw.y) : 0;

        bool xSignChanged = xDown && xWasDown && (xSign != lastXSign);
        bool ySignChanged = yDown && yWasDown && (ySign != lastYSign);

        // 디지털 입력: 마지막 축 우선
        UpdateActiveAxis(xDown, yDown, xNewDown, yNewDown, xSignChanged, ySignChanged);

        // 축별 처리
        if (activeAxis == NavAxis.Y)
        {
            HandleYInput(yDown, ySign, yNewDown, ySignChanged);
        }
        else if (activeAxis == NavAxis.X)
        {
            HandleXInput(xDown, xSign, xNewDown, xSignChanged);
        }

        lastRaw = raw;
    }

    public override void OnUIInputConfirm()
    {
        if (focusLocked) return;

        StopHold();

        var cur = items[currentIndex];
        if (cur.CanSubmit)
            cur.Submit();
    }
    public override void OnUIInputCancel()
    {
        RequestClose();
    }

    public void RequestClose()
    {
        if (isAskingClose) return;

        if (SettingsManager.IsDirty)
        {
            isAskingClose = true;
            alertPanel_SettingSave.Open();
            return;
        }

        Close();
    }

    // setting save ConfirmPanel "예/확인" 버튼에 연결
    public void OnSettingSaveConfirm()
    {
        SettingsManager.CommitAndSave();
        alertPanel_SettingSave.Close();
        isAskingClose = false;

        Close();
    }

    // setting save ConfirmPanel "아니오/취소" 버튼에 연결
    public void OnSettingSaveCancel()
    {
        SettingsManager.RevertWorkingToCommitted();
        alertPanel_SettingSave.Close();
        isAskingClose = false;

        Close();
    }
    #endregion

    #region Input Helpers
    void UpdateActiveAxis(bool xDown, bool yDown, bool xNewDown, bool yNewDown, bool xSignChanged, bool ySignChanged)
    {
        // 1) 새로 눌린 축이 있으면 그 축으로 전환
        if (xNewDown) activeAxis = NavAxis.X;
        else if (yNewDown) activeAxis = NavAxis.Y;

        // 2) 같은 축에서 방향이 바뀐 경우는 축 유지
        else if (xSignChanged && activeAxis == NavAxis.X) { }
        else if (ySignChanged && activeAxis == NavAxis.Y) { }

        // 3) 아직 축이 없다면(처음 입력) X 우선, 없으면 Y
        else if (activeAxis == NavAxis.None)
        {
            if (xDown) activeAxis = NavAxis.X;
            else if (yDown) activeAxis = NavAxis.Y;
        }
    }

    void HandleYInput(bool yDown, int ySign, bool yNewDown, bool ySignChanged)
    {
        // X 조절 홀드 중이면 끊고 Y 네비로 전환
        if (holdX != 0) StopHold();

        if (!yDown)
        {
            // Stop Nav Hold
            holdY = 0;
            holdElapsed = 0f;
            nextFireTime = 0f;
            return;
        }

        if (yNewDown || ySignChanged)
            StartNavHold(ySign); // 즉시 1회 + 이후 일정 간격 반복
    }
    
    void HandleXInput(bool xDown, int xSign, bool xNewDown, bool xSignChanged)
    {
        if (!xDown)
        {
            StopHold();
            return;
        }

        if (!(xNewDown || xSignChanged))
            return;

        var cur = items[currentIndex];

        // 좌/우 네비가 "실제로 존재"하면 그걸 우선
        if (HasHorizontalNav(xSign))
        {
            StopHold();
            TryMoveByMap(xSign: xSign, ySign: 0);
            return;
        }

        // 좌/우 네비가 없으면 조절 가능한 항목만 값 조절
        if (cur.CanAdjust)
            StartAdjustHold(xSign);
    }
    #endregion

    #region Navigation
    protected virtual int GetDefaultIndex() => 0;
    protected virtual void BuildNavMap()
    {

    }

    bool HasHorizontalNav(int xSign)
    {
        if (!navMap.TryGetValue(currentIndex, out var n))
            return false;

        if (xSign < 0) return n.left != currentIndex;
        if (xSign > 0) return n.right != currentIndex;
        return false;
    }

    bool TryMoveByMap(int xSign, int ySign)
    {
        if (!navMap.TryGetValue(currentIndex, out var n))
            return false;

        int next = currentIndex;

        if (ySign > 0) next = n.up;
        else if (ySign < 0) next = n.down;
        else if (xSign < 0) next = n.left;
        else if (xSign > 0) next = n.right;

        if (next == currentIndex)
            return false;

        if (!IsValidIndex(next))
            return false;

        items[currentIndex].SetSelected(false);
        currentIndex = next;
        items[currentIndex].SetSelected(true);
        return true;
    }

    void StartNavHold(int dirY)
    {
        if (holdY == dirY) return;

        holdY = dirY;

        // 즉시 1회 이동
        FireNavOnce();

        // 이후 반복 스케줄
        holdElapsed = 0f;
        nextFireTime = navHoldStartDelay;
    }

    void FireNavOnce()
    {
        TryMoveByMap(0, holdY);
    }

    void StartAdjustHold(int dirX)
    {
        var cur = items[currentIndex];

        // Adjust 불가면 홀드 의미 없음
        if (!cur.CanAdjust)
        {
            StopHold();
            return;
        }

        // 같은 방향 + 같은 아이템 홀드 중이면 무시
        if (holdX == dirX && holdItem == cur)
            return;

        holdX = dirX;
        holdItem = cur;

        // 즉시 1회 조절
        FireAdjustOnce();

        holdElapsed = 0f;

        if (cur.RepeatMode == UIRepeatMode.FixedInterval)
        {
            nextFireTime = cur.RepeatInterval;
        }
        else if (cur.RepeatMode == UIRepeatMode.Accelerate)
        {
            nextFireTime = cur.AccelStartDelay;
            accelInterval = cur.AccelInitialInterval;
        }
        else
        {
            StopHold();
        }
    }

    void FireAdjustOnce()
    {
        holdItem.Adjust(holdX);
    }

    void ScheduleAdjustNext()
    {
        holdElapsed = 0f;

        if (holdItem.RepeatMode == UIRepeatMode.FixedInterval)
        {
            nextFireTime = holdItem.RepeatInterval;
            return;
        }

        if (holdItem.RepeatMode == UIRepeatMode.Accelerate)
        {
            accelInterval = Mathf.Max(holdItem.AccelMinInterval, accelInterval * holdItem.AccelFactor);
            nextFireTime = accelInterval;
        }
    }

    void StopHold()
    {
        holdX = 0;
        holdY = 0;
        holdElapsed = 0f;
        nextFireTime = 0f;
        accelInterval = 0f;
        holdItem = null;
    }
    #endregion

    #region 내부 함수
    void BuildItemCache()
    {
        items.Clear();
        indexByItem.Clear();

        foreach (var go in settingItemObjects)
        {
            if (go == null) continue;
            if (!go.activeInHierarchy) continue;

            if (!go.TryGetComponent<ISettingItem>(out var item))
                continue;

            items.Add(item);
        }

        for (int i = 0; i < items.Count; i++)
            indexByItem[items[i]] = i;
    }
    void RefreshSelection()
    {
        for (int i = 0; i < items.Count; i++)
            items[i].SetSelected(i == currentIndex);
    }

    bool IsValidIndex(int idx)
    {
        return idx >= 0 && idx < items.Count;
    }

    void ResetInputState()
    {
        lastRaw = Vector2.zero;
        activeAxis = NavAxis.None;
        StopHold();
    }
    #endregion

    #region Button Actions
    public void GotoTitle()
    {
        if(SoundManager.Instance != null) SoundManager.Instance.StopBGM();
        SceneLoader.LoadScene("Start");
    }

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnOpened();
    }
    #endregion
}

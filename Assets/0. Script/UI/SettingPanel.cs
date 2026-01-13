using System.Collections.Generic;
using UnityEngine;

public class SettingPanel : UIPanelBase
{
    [Header("Setting Items (Top → Bottom)")]
    [SerializeField] List<MonoBehaviour> settingItemBehaviours;

    [SerializeField] SaveAlertPanel saveAlertPanel_GotoTitle;
    [SerializeField] SaveAlertPanel saveAlertPanel_QuitGame;

    List<ISettingItem> items = new();
    Dictionary<ISettingItem, int> indexByItem = new();
    int currentIndex;

    public void OpenSaveAlertPanel_GotoTitle()
    {
        saveAlertPanel_GotoTitle.Open();
    }
    public void OpenSaveAlertPanel_QuitGame()
    {
        saveAlertPanel_QuitGame.Open();
    }

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

        currentIndex = 3; // 저장 버튼을 기본값.

        RefreshSelection();

        saveAlertPanel_GotoTitle.gameObject.SetActive(true);
        saveAlertPanel_QuitGame.gameObject.SetActive(true);

        saveAlertPanel_GotoTitle.Close();
        saveAlertPanel_QuitGame.Close();
    }

    public void RequestFocus(ISettingItem item)
    {
        if (item == null)
            return;

        if (!indexByItem.TryGetValue(item, out int idx))
            return;

        if (idx == currentIndex)
            return;

        items[currentIndex].SetSelected(false);
        currentIndex = idx;
        items[currentIndex].SetSelected(true);
    }

    public override void OnUIInputMove(Vector2 dir)
    {
        // 위 / 아래 : 항목 이동
        if (Mathf.Abs(dir.y) > 0.1f)
        {
            MoveIndex(dir.y > 0 ? -1 : +1);
            return;
        }

        // 좌 / 우 : 값 변경
        if (Mathf.Abs(dir.x) > 0.1f)
        {
            var cur = items[currentIndex];
            if (cur.CanAdjust)
                cur.Adjust(dir.x > 0 ? +1 : -1);
        }
    }

    public override void OnUIInputConfirm()
    {
        var cur = items[currentIndex];
        if (cur.CanSubmit)
            cur.Submit();
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

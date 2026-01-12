using System.Collections.Generic;
using UnityEngine;

public class SettingPanel : UIPanelBase
{
    [Header("Setting Items (Top → Bottom)")]
    [SerializeField] List<MonoBehaviour> settingItemBehaviours;

    List<ISettingItem> items = new();
    int? currentIndex;

    protected override void Init()
    {
        items.Clear();
        foreach (var mb in settingItemBehaviours)
        {
            if (mb is ISettingItem item)
                items.Add(item);
        }

        if(currentIndex == null) return;
        currentIndex = Mathf.Clamp((int)currentIndex, 0, items.Count - 1);
        RefreshSelection();
    }

    public override void OnUIInputMove(Vector2 dir)
    {
        // 위 / 아래 : 항목 이동
        if (Mathf.Abs(dir.y) > 0.1f)
        {
            MoveIndex(dir.y > 0 ? -1 : +1);
            return;
        }

        if(currentIndex == null) return;
        // 좌 / 우 : 값 변경
        if (Mathf.Abs(dir.x) > 0.1f)
        {
            var cur = items[(int)currentIndex];
            if (cur.CanAdjust)
                cur.Adjust(dir.x > 0 ? +1 : -1);
        }
    }

    public override void OnUIInputConfirm()
    {
        if(currentIndex == null) return;
        var cur = items[(int)currentIndex];
        if (cur.CanSubmit)
            cur.Submit();
    }

    void MoveIndex(int delta)
    {
        if(currentIndex == null) return;
        items[(int)currentIndex].SetSelected(false);
        currentIndex = (currentIndex + delta + items.Count) % items.Count;
        items[(int)currentIndex].SetSelected(true);
    }

    void RefreshSelection()
    {
        if(currentIndex == null) return;
        for (int i = 0; i < items.Count; i++)
            items[i].SetSelected(i == currentIndex);
    }

    protected override void OnClosing()
    {
        currentIndex = null;
    }
}
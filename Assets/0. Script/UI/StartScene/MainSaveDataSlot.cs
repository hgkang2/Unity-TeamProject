using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainSaveDataSlot : MonoBehaviour
{
    [SerializeField] CanvasGroup newTravelCG;
    [SerializeField] CanvasGroup loadTravelCG;

    [SerializeField] Image CharacterImage;
    [SerializeField] TMP_Text ChatacterName;
    [SerializeField] TMP_Text ChatacterLevel;
    [SerializeField] TMP_Text PlayTime;
    [SerializeField] TMP_Text Chapter;

    public RectTransform Rect { get; private set; }

    SaveData saveData;
    public SaveData SaveData => saveData;

    public int myIndex;

    public event Action<int> slotselected;
    public void RaiseSlotSelected() => slotselected?.Invoke(myIndex);
    public event Action<int> slotFocused;
    public void RaiseSlotFocused() => slotFocused?.Invoke(myIndex);
    public event Action<int> slotUnFocused;
    public void RaiseSlotUnFocused() => slotUnFocused?.Invoke(myIndex);

    void Awake()
    {
        Rect = transform as RectTransform;
    }

    public void Bind(SaveData data)
    {
        // TODO 불러오기 로직 적용후 채우기!!!
        saveData = data;
        if (saveData == null)
        {
            Clear();
        }
        else
        {
            loadTravelCG.alpha = 1;
            loadTravelCG.blocksRaycasts = true;
            loadTravelCG.interactable = true;

            newTravelCG.alpha = 1;
            newTravelCG.blocksRaycasts = false;
            newTravelCG.interactable = false;
        }
    }

    public void Clear()
    {
        loadTravelCG.alpha = 0;
        loadTravelCG.blocksRaycasts = false;
        loadTravelCG.interactable = false;

        newTravelCG.alpha = 1;
        newTravelCG.blocksRaycasts = true;
        newTravelCG.interactable = true;
    }


    public float hoverScale = 1.05f;
    public float tweenDuration = 0.15f;
    public void Focused()
    {
        Rect.DOKill(); // 기존 트윈 제거
        Rect.DOScale(hoverScale, tweenDuration)
            .SetEase(Ease.OutBack);
    }

    public void UnFocused()
    {
        Rect.DOKill();
        Rect.DOScale(0.95f, tweenDuration)
            .SetEase(Ease.OutQuad);
    }
}

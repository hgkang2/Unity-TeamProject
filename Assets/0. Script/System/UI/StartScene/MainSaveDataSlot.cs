using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainSaveDataSlot : MonoBehaviour, IInteractiveView<SaveData>
{
    [SerializeField] CanvasGroup newTravelCG;
    [SerializeField] CanvasGroup loadTravelCG;

    [SerializeField] Image CharacterImage;
    [SerializeField] TMP_Text ChatacterName;
    [SerializeField] TMP_Text ChatacterLevel;
    [SerializeField] TMP_Text PlayTime;
    [SerializeField] TMP_Text Chapter;


    public UIPointerHandler PointerHandler { get; private set; }
    public UIClickHandler ClickHandler { get; private set; }
    public UIDragHandler DragHandler => null;

    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;

    SaveData saveData;

    void Awake()
    {
        Rect = transform as RectTransform;

        UIPointerHandler<SaveData> myPointerHandler = GetComponent<UIPointerHandler<SaveData>>();
        PointerHandler = myPointerHandler;

        myPointerHandler.GetData = () => saveData;
        myPointerHandler.GetRect = () => Rect;

        myPointerHandler.PointerEntered += HandlePointerEnter;
        myPointerHandler.PointerExited += HandlePointerExit;


        UIClickHandler<SaveData> myClickHandler = GetComponent<UIClickHandler<SaveData>>();
        ClickHandler = myClickHandler;

        myClickHandler.GetData = () => saveData;


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


    void HandlePointerEnter(SaveData data, RectTransform rect, PointerEventData eventData)
    {
        Selected();
    }

    void HandlePointerExit()
    {
        DeSelected();
    }

    
    public float hoverScale = 1.05f;
    public float tweenDuration = 0.15f;
    void Selected()
    {
        Rect.DOKill(); // 기존 트윈 제거
        Rect.DOScale(hoverScale, tweenDuration)
            .SetEase(Ease.OutBack);
    }

    void DeSelected()
    {
        Rect.DOKill();
        Rect.DOScale(0.95f, tweenDuration)
            .SetEase(Ease.OutQuad);
    }
}

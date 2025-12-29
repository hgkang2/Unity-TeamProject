using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainLoadSlot : MonoBehaviour, IInteractiveView<SaveData>
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

    UIPointerHandler<SaveData> myPointerHandler;

    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;

    SaveData saveData = new SaveData();

    void Awake()
    {
        Rect = transform as RectTransform;

        myPointerHandler = GetComponent<UIPointerHandler<SaveData>>();
        PointerHandler = myPointerHandler;

        // 핸들러가 데이터를 읽어갈 수 있도록 델리게이트 연결
        myPointerHandler.GetData = () => saveData;
        myPointerHandler.GetRect = () => Rect;

        // 이제 구독 가능
        myPointerHandler.PointerEntered += HandlePointerEnter;
        myPointerHandler.PointerExited += HandlePointerExit;
    }


    public void Bind(SaveData data)
    {
        //saveData = new SaveData();
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

    void HandlePointerEnter(SaveData data, RectTransform rect, PointerEventData eventData)
    {
        rect.DOKill(); // 기존 트윈 제거
        rect.DOScale(hoverScale, tweenDuration)
            .SetEase(Ease.OutBack);
    }

    void HandlePointerExit()
    {
        RectTransform rect = Rect; // 슬롯 자기 자신
        rect.DOKill();
        rect.DOScale(0.95f, tweenDuration)
            .SetEase(Ease.OutQuad);
    }

}

using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : UIKeyboardHandler
{
    public CanvasGroup cg;

    Button[] menuButtons;
    [SerializeField] Transform ButtonSelectImage;

    private void Awake()
    {
        int i = 0;
        menuButtons = transform.Cast<Transform>()
            .Select(t =>
            {
                Button button = t.GetComponent<Button>();
                StartButtonEvent evt = t.GetComponent<StartButtonEvent>();

                if (evt == null) return null;
                if (!evt.activate) return null;

                evt.SetIndex(i++);
                evt.onEnter += ButtonMouseEnter;
                evt.onExit += ButtonMouseExit;
                return button;
            })
            .Where(b => b != null)
            .ToArray();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        ButtonSelectImage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {

        for (int i = 0; i < menuButtons.Length; i++)
        {
            StartButtonEvent evt = menuButtons[i].GetComponent<StartButtonEvent>();
            if (evt == null) continue;

            evt.onEnter -= ButtonMouseEnter;
            evt.onExit -= ButtonMouseExit;
        }
    }

    int? curIndex = null;
    void UpdateButtonHighlight()
    {
        if (curIndex == null)
        {
            ButtonSelectImage.gameObject.SetActive(false);
        }
        else
        {
            ButtonSelectImage.gameObject.SetActive(true);
            ButtonSelectImage.transform.position = menuButtons[(int)curIndex].transform.position;
        }
    }

    public void ButtonMouseEnter(int index)
    {
        curIndex = index;
        UpdateButtonHighlight();
    }

    public void ButtonMouseExit()
    {
        ButtonSelectImage.gameObject.SetActive(false);
    }

    protected override void OnUIMove(Vector2 dir)
    {
        // 현재 아무것도 선택되지 않은 상태라면
        if (curIndex == null)
        {
            // 왼쪽/위쪽 → 0번 선택
            if (dir.x < -0.1f || dir.y > 0.1f) curIndex = 0;
            // 오른쪽/아래쪽 → 마지막 선택
            else if (dir.x > 0.1f || dir.y < -0.1f) curIndex = menuButtons.Length - 1;

            UpdateButtonHighlight();
            return;
        }

        //왼쪽 or 위쪽 방향키시 위쪽 방향으로
        if (dir.x < -0.1f || dir.y > 0.1) curIndex--;
        //오른쪽 or 아래쪽 방향키시 아래쪽 방향으로
        else if (dir.x > 0.1f || dir.y < -0.1) curIndex++;

        //min, max 처리
        if (curIndex < 0) curIndex = 0;
        else if (curIndex >= menuButtons.Length) curIndex = menuButtons.Length - 1;

        //강조된 버튼 변경
        UpdateButtonHighlight();
    }
    //버튼 선택만 해제
    public void QuitButtonSelect()
    {
        if (curIndex != null)
        {
            curIndex = null;
            UpdateButtonHighlight();
        }
    }

    protected override void OnUIConfirm()
    {
        //선택 버튼이 없으면 무시
        if (curIndex == null) return;
        // 마우스로 클릭한 것과 동일하게 실행
        menuButtons[curIndex.Value].onClick.Invoke();
    }

    protected override void OnUICancel()
    {
        // 버튼이 선택된 상태면 해제
        if (curIndex != null)
        {
            curIndex = null;
            UpdateButtonHighlight();
        }
    }

    public void Open()
    {
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        enabled = true;
    }

    public void Close()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        enabled = false;
    }
}

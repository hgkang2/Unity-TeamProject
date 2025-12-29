using UnityEngine;

public class MainCharacterChoicePanel : UIKeyboardHandler
{
    CanvasGroup cg;
    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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

    protected override void OnUIMove(Vector2 dir)
    {

    }
}

using UnityEngine;

public class MainCharacterChoicePanel : UIKeyboardHandler
{
    [HideInInspector] public CanvasGroup cg;
    [SerializeField] MainCharacterChoiceSlot[] slots;
    void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        foreach(var slot in slots)
        {
            slot.characterSelected += GameStart;
        }
    }


    // TODO 확인창 이후 Start로 바꾸기?
    public void GameStart(CharacterId id)
    {
        GameManager.Instance.curcharacter = id;
        SceneLoader.LoadScene("Stage1");
        
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

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterChoiceUI : MonoBehaviour
{
    CharacterChoicePanel[] characterChoicePanels;
    [SerializeField] Transform selectPanel;
    [SerializeField] TMP_Text characterNameText;

    void Awake()
    {
        characterChoicePanels = GetComponentsInChildren<CharacterChoicePanel>();
        for(int i=0; i<characterChoicePanels.Length; i++)
        {
            characterChoicePanels[i].Clicked += ShowSelectPanel;
        }
        HideSelectPanel();
    }

    public void ShowSelectPanel(int characterId)
    {
        SelectedCharacter.CurCharacter = (CharacterId)characterId;
        characterNameText.text = SelectedCharacter.CurCharacter.ToString();

        selectPanel.gameObject.SetActive(true);
    }
    public void HideSelectPanel()
    {
        selectPanel.gameObject.SetActive(false);
    }
}

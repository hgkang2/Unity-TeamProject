using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StageUI : MonoBehaviour
{
    [SerializeField] TMP_Text characterName;

    //esc 일시 정지 창
    [SerializeField] GameObject escpanel;

    //esc 하위의 설정 창
    [SerializeField] GameObject settingPanel;

    //레벨업 이벤트 구독 용
    [SerializeField] Exp exp;

    //레벨업 시 띄우기
    [SerializeField] LevelUpPanel levelUpPanel;

    //보유 영성 ui
    [SerializeField] HaveSoulsPanel haveSoulsPanel;

    void Awake()
    {
        //이벤트 구독
        exp.LevelUpped += HandleLevelUp;
        levelUpPanel.SelectSoulCompleted += HideLevelupPanel;

        //기본적으로 모든 ui 한번 열었다 닫기(초기화용)
        ShowEscPanel();
        ShowSettingPanel();
        ShowLevelupPanel();
        haveSoulsPanel.gameObject.SetActive(true);
        

        characterName.text = SelectedCharacter.CurCharacter.ToString();
    }

    void Start()
    {
        //모든 ui 닫기
        HideEscPanel();
        HideSettingPanel();
        HideLevelupPanel();
        haveSoulsPanel.gameObject.SetActive(false);
    }
    void OnDestroy()
    {
        exp.LevelUpped -= HandleLevelUp;
        levelUpPanel.SelectSoulCompleted -= HideLevelupPanel;
    }


    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Z))
        // {
        //     haveSoulsPanel.gameObject.SetActive(true);
        // }
        // if (Input.GetKeyUp(KeyCode.Z))
        // {
        //     haveSoulsPanel.gameObject.SetActive(false);
        // }
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     //우선 설정창 켜져있으면 닫기
        //     if (settingPanel.activeSelf)
        //     {
        //         HideSettingPanel();
        //         return;
        //     }

        //     //아니면 일시정지 / 풀기
        //     if (!PauseManager.IsPaused)
        //     {
        //         ShowEscPanel();
        //         PauseManager.Pause();
        //     }
        //     else
        //     {
        //         HideEscPanel();
        //         PauseManager.Resume();
        //     }
        // }
    }

    public void ShowEscPanel()
    {
        escpanel.gameObject.SetActive(true);
    }

    public void HideEscPanel()
    {
        escpanel.gameObject.SetActive(false);
        HideSettingPanel();
    }

    //버튼 연결 이벤트
    public void ShowSettingPanel()
    {
        settingPanel.gameObject.SetActive(true);
    }

    public void HideSettingPanel()
    {
        settingPanel.gameObject.SetActive(false);
    }

    void HandleLevelUp()
    {
        ShowLevelupPanel();
        levelUpPanel.Reroll();
    }
    public void ShowLevelupPanel()
    {
        levelUpPanel.gameObject.SetActive(true);
        PauseManager.Pause();
    }
    public void HideLevelupPanel()
    {
        levelUpPanel.gameObject.SetActive(false);
        PauseManager.Resume();
    }
    //버튼 연결 이벤트
    public void GoToCharacterChoiceScene()
    {
        SelectedCharacter.CurCharacter = CharacterId.None;
        SceneManager.LoadScene("CharacterChoice");
    }
}

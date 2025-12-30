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
    [SerializeField] GameObject haveSoulsUI;

    void Awake()
    {
        //이벤트 구독
        exp.LevelChanged += HandleLevelUp;
        levelUpPanel.SelectSoulCompleted += HideLevelupPanel;

        //기본적으로 모든 ui 한번 열었다 닫기(초기화용)
        escpanel.gameObject.SetActive(true);
        settingPanel.gameObject.SetActive(true);
        levelUpPanel.gameObject.SetActive(true);
        haveSoulsUI.gameObject.SetActive(true);
        

        characterName.text = GameManager.Instance.curcharacter.ToString();
    }

    void Start()
    {
        //모든 ui 닫기
        escpanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
        levelUpPanel.gameObject.SetActive(false);
        haveSoulsUI.gameObject.SetActive(false);
    }
    void OnDestroy()
    {
        exp.LevelChanged -= HandleLevelUp;
        levelUpPanel.SelectSoulCompleted -= HideLevelupPanel;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            haveSoulsUI.gameObject.SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            haveSoulsUI.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //우선 설정창 켜져있으면 닫기
            if (settingPanel.activeSelf)
            {
                HideSettingPanel();
                return;
            }

            //아니면 일시정지 / 풀기
            if (!TimeManager.IsPaused)
            {
                ShowEscPanel();
                TimeManager.Pause();
            }
            else
            {
                HideEscPanel();
                TimeManager.Resume();
            }
        }
    }

    public void ShowEscPanel()
    {
        TimeManager.Pause();

        escpanel.gameObject.SetActive(true);
    }

    public void HideEscPanel()
    {
        escpanel.gameObject.SetActive(false);
        HideSettingPanel();

        TimeManager.Resume();
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

    
    void HandleLevelUp(int newCurLevel)
    {
        ShowLevelupPanel();
    }
    public void ShowLevelupPanel()
    {
        TimeManager.Pause();

        levelUpPanel.gameObject.SetActive(true);
        levelUpPanel.Initialize();
    }
    public void HideLevelupPanel()
    {
        levelUpPanel.gameObject.SetActive(false);

        TimeManager.Resume();
    }
    //버튼 연결 이벤트
    public void GoToCharacterChoiceScene()
    {
        GameManager.Instance.curcharacter = CharacterId.None;
        SceneManager.LoadScene("CharacterChoice");
    }
}

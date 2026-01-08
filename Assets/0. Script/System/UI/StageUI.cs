using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StageUI : MonoBehaviour
{
    [SerializeField] TMP_Text characterName;

    //esc 일시 정지 창
    [SerializeField] MonoBehaviour escpanel;

    //esc 하위의 설정 창
    [SerializeField] MonoBehaviour settingPanel;

    //레벨업 이벤트 구독 용
    Exp exp;
    //레벨업 시 띄우기
    LevelUpPanel levelUpPanel;

    //보유 영성 ui
    HaveSoulsPanel haveSoulsPanel;

    readonly System.Collections.Generic.Stack<IOpenStackUI> uiStack = new();

    void Awake()
    {
        SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
        levelUpPanel = sceneContext.levelUpPanel;
        haveSoulsPanel = sceneContext.haveSoulsPanel;

        //이벤트 구독
        sceneContext.player.Exp.LevelChanged += HandleLevelUp;
        levelUpPanel.SelectSoulCompleted += HideLevelupPanel;

        //기본적으로 모든 ui 활성화시켜놓기
        escpanel.gameObject.SetActive(true);
        settingPanel.gameObject.SetActive(true);
        levelUpPanel.gameObject.SetActive(true);
        haveSoulsPanel.gameObject.SetActive(true);
        

        characterName.text = GameManager.Instance.curcharacter.ToString();
    }

    void Start()
    {
        //모든 ui 닫기(닫기 말고 CanvasGroup으로 hide 하게 수정하기)
        escpanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
        levelUpPanel.Hide();
        haveSoulsPanel.Hide();
    }
    void OnDestroy()
    {
        SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
        sceneContext.player.Exp.LevelChanged -= HandleLevelUp;
        levelUpPanel.SelectSoulCompleted -= HideLevelupPanel;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            haveSoulsPanel.Show();
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            haveSoulsPanel.Hide();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //우선 설정창 켜져있으면 닫기
            // if (settingPanel)
            // {
            //     HideSettingPanel();
            //     return;
            // }

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
        levelUpPanel.Show();
        levelUpPanel.StartAnim();
    }
    public void HideLevelupPanel()
    {
        levelUpPanel.Hide();
        TimeManager.Resume();
    }
    //버튼 연결 이벤트
    public void GoToCharacterChoiceScene()
    {
        GameManager.Instance.curcharacter = CharacterId.None;
        SceneManager.LoadScene("Start");
    }
}

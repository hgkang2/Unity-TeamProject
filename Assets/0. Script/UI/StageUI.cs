using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StageUI : MonoBehaviour
{
    //esc 일시 정지 창
    IngameSettingPanel settingPanel;
    //레벨업 시 띄우기
    LevelUpPanel levelUpPanel;
    //보유 영성 ui
    HaveSoulPanel haveSoulsPanel;


    SceneContext sceneContext;
    void Awake()
    {
        sceneContext = FindFirstObjectByType<SceneContext>();
        levelUpPanel = sceneContext.levelUpPanel;
        haveSoulsPanel = sceneContext.haveSoulsPanel;
        settingPanel = sceneContext.ingameSettingPanel;

        //기본적으로 모든 ui 활성화시켜놓기
        settingPanel.gameObject.SetActive(true);
        levelUpPanel.gameObject.SetActive(true);
        haveSoulsPanel.gameObject.SetActive(true);
    }

    void Start()
    {
        //모든 ui 닫기
        settingPanel.Close();
        levelUpPanel.Close();
        haveSoulsPanel.Close();
    }

    void OnEnable()
    {
        InputManager.Instance.EscPressed += OnEscPressed;
        InputManager.Instance.ZPressed += OnZPressed;
        InputManager.Instance.ZReleased += OnZReleased;
    }

    void OnDisable()
    {
        InputManager.Instance.EscPressed -= OnEscPressed;
        InputManager.Instance.ZPressed -= OnZPressed;
        InputManager.Instance.ZReleased -= OnZReleased;
    }


    void OnEscPressed()
    {
        settingPanel.Open();
    }

    void OnZPressed()
    {
        haveSoulsPanel.Open();
    }

    void OnZReleased()
    {
        haveSoulsPanel.Close();
    }


    // 개발자 도구 - '캐릭터 선택창으로 버튼'에 연결
    public void GoToCharacterChoiceScene()
    {
        GameManager.Instance.curcharacter = CharacterId.None;
        SceneManager.LoadScene("Start");
    }
}
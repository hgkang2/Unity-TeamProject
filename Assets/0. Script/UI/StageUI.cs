using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StageUI : MonoBehaviour
{

    [SerializeField] TMP_Text characterName;

    //esc 일시 정지 창
    EscPanel escPanel;
    //레벨업 시 띄우기
    LevelUpPanel levelUpPanel;
    //보유 영성 ui
    HaveSoulsPanel haveSoulsPanel;


    SceneContext sceneContext;
    void Awake()
    {
        sceneContext = FindFirstObjectByType<SceneContext>();
        levelUpPanel = sceneContext.levelUpPanel;
        haveSoulsPanel = sceneContext.haveSoulsPanel;
        escPanel = sceneContext.escPanel;

        //기본적으로 모든 ui 활성화시켜놓기
        escPanel.gameObject.SetActive(true);
        levelUpPanel.gameObject.SetActive(true);
        haveSoulsPanel.gameObject.SetActive(true);

        //모든 ui 닫기
        escPanel.Close();
        levelUpPanel.Close();
        haveSoulsPanel.Close();

        characterName.text = GameManager.Instance.curcharacter.ToString();
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
        escPanel.Open();
        TimeManager.Pause();
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
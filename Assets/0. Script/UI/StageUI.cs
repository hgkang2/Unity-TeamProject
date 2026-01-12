using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StageUI : UIPanelBase
{
    
    [SerializeField] TMP_Text characterName;

    //esc 일시 정지 창
    EscPanel escPanel;



    //레벨업 시 띄우기
    LevelUpPanel levelUpPanel;

    //보유 영성 ui
    HaveSoulsPanel haveSoulsPanel;


    SceneContext sceneContext;
    protected override void Init()
    {
        sceneContext = FindFirstObjectByType<SceneContext>();
        haveSoulsPanel = sceneContext.haveSoulsPanel;
        escPanel = sceneContext.escPanel;

        //기본적으로 모든 ui 활성화시켜놓기
        escPanel.gameObject.SetActive(true);
        haveSoulsPanel.gameObject.SetActive(true);

        //모든 ui 닫기
        escPanel.Close();
        haveSoulsPanel.Close();


        characterName.text = GameManager.Instance.curcharacter.ToString();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            haveSoulsPanel.Open();
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            haveSoulsPanel.Close();
        }
    }

    //버튼 연결 이벤트
    public void GoToCharacterChoiceScene()
    {
        GameManager.Instance.curcharacter = CharacterId.None;
        SceneManager.LoadScene("Start");
    }

    public override void OnUIInputCancel()
    {
        escPanel.Open();
        TimeManager.Pause();
    }
}
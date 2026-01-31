using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class AltarUI : UIPanelBase
{
    Altar altar;
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text flameUseValueText;
    [SerializeField] MyButton purifyButton;

    [HideInInspector] public Interactor interactor;

    protected override void Init()
    {
        Close();
    }

    protected override void OnOpened()
    {
        slider.minValue = 0;
        slider.maxValue = GameManager.Instance.HasFlame;
        slider.value = 0;
        purifyButton.DeActivate();

        GameManager.Instance.changedHasFlame += SetFlameMaxValue;
    }

    protected override void OnClosing()
    {
        GameManager.Instance.changedHasFlame -= SetFlameMaxValue;
        interactor.InteractExit();
        interactor = null;
    }


    bool isButton = false;
    public override void OnUIInputMove(Vector2 dir)
    {
        base.OnUIInputMove(dir);
        //위 방향키(슬라이더 포커스)
        if(dir.y > 0.1)
        {
            isButton = false;
        }
        //아래 방향키(정화버튼 포커스)
        else if(dir.y < -0.1)
        {
            isButton = true;
        }

        // 현재 슬라이더 포커스면 좌우 이동 가능
        if(isButton == false)
        {
            if(dir.x < -0.1) slider.value--;
            else if(dir.x > 0.1) slider.value++;
        }
    }

    public override void OnUIInputConfirm()
    {
        purifyButton.Click();
    }


    // ui 진입 이후 불꽃 변동 시에도 반영되게
    void SetFlameMaxValue()
    {
        slider.maxValue = GameManager.Instance.HasFlame;
    }


    // 슬라이더 값 바뀔 때마다 적용
    public void UseFlameNumSetting()
    {
        int useNum = (int)slider.value;
        flameUseValueText.SetText($"{useNum}");
        if(useNum == 0)
        {
            purifyButton.DeActivate();
        }
        else
        {
            purifyButton.Activate();
        }
    }

    public void TryPurify()
    {
        GameManager.Instance.AlterPurify((int)slider.value);
        Close();
    }
    public void Purify()
    {
        GameManager.Instance.ActivateAlter();
        Close();
    }
}

using System.Collections;
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


    GameManager GM;
    Coroutine bindCo;
    bool bound;

    protected override void OnOpened()
    {
        if (bindCo == null)
            bindCo = StartCoroutine(InstanceDescribe());
    }

    protected override void OnClosing()
    {
        if (bindCo != null)
        {
            StopCoroutine(bindCo);
            bindCo = null;
        }

        if (bound && GM != null)
        {
            GM.changedHasFlame -= SetFlameMaxValue;
            bound = false;
        }

        interactor.InteractExit();
        interactor = null;
    }

    IEnumerator InstanceDescribe()
    {
        while (GameManager.Instance == null)
            yield return null;

        GM = GameManager.Instance;

        // 중복 구독 방지(가장 간단한 방법)
        if (!bound)
        {
            GM.changedHasFlame += SetFlameMaxValue;
            bound = true;
        }

        slider.minValue = 0;
        slider.maxValue = GM.HasFlame;
        slider.value = 0;
        purifyButton.DeActivate();

        bindCo = null;
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
        purifyButton.Click(); // => TryPurify()
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

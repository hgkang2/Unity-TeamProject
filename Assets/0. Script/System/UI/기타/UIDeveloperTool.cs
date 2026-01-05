using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIDeveloperTool : MonoBehaviour
{
    //Heal
    [SerializeField] Slider HealSlider;
    [SerializeField] TMP_Text HealValueText;
    public void ChangeHealValueText()
    {
        HealValueText.text = ((int)(HealSlider.value)).ToString();
    }

    //Take Damage
    [SerializeField] Slider takeDamageSlider;
    [SerializeField] TMP_Text takeDamageValueText;
    public void ChangeTakeDamageValueText()
    {
        takeDamageValueText.text = ((int)(takeDamageSlider.value)).ToString();
    }
    //Get Exp
    [SerializeField] Slider getExpSlider;
    [SerializeField] TMP_Text getExpValueText;
    public void ChangeGetExpValueText()
    {
        getExpValueText.text = ((int)(getExpSlider.value)).ToString();
    }

    //Change TimeScale
    [SerializeField] Slider getTimeScaleSlider;
    [SerializeField] TMP_Text getTimeScaleText;
    public void ChangeTimeScaleValueText()
    {
        getTimeScaleText.text = ((getTimeScaleSlider.value)).ToString();
        TimeManager.SetTimeScale(getTimeScaleSlider.value);
    }

    Player player;

    private void Awake() {
        player = FindFirstObjectByType<Player>();
    }

    void Start()
    {
        ChangeHealValueText();
        ChangeTakeDamageValueText();
        ChangeGetExpValueText();
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (HealSlider.isActiveAndEnabled)
                player.Heal(HealSlider.value);
            else
                player.Heal(10);

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (takeDamageSlider.isActiveAndEnabled)
                player.TakeDamage(takeDamageSlider.value, DamageType.Normal);
            else
                player.TakeDamage(takeDamageSlider.value, DamageType.Normal);

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (getExpSlider.isActiveAndEnabled)
                player.Exp.AddExp((int)getExpSlider.value);
            else
                player.Exp.AddExp(10);

        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            player.Exp.LevelUp();
        }
    }
}

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

    [SerializeField] Player player;

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
            player.HP.Heal(HealSlider.value);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            player.HP.TakeDamage(takeDamageSlider.value);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            player.Exp.AddExp((int)getExpSlider.value);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            player.Exp.LevelUp();
        }
    }
}

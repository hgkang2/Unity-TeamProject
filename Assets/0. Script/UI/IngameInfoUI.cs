using TMPro;
using UnityEngine;

public class IngameInfoUI : MonoBehaviour
{
    [SerializeField] TMP_Text stageTimeText;
    [SerializeField] TMP_Text ingameTimeText;
    [SerializeField] TMP_Text stageNumText;
    [SerializeField] TMP_Text flameNumText;

    void OnEnable()
    {
        GameManager.Instance.changedHasFlame += SetFlameValueText;
    }

    void OnDisable()
    {
        GameManager.Instance.changedHasFlame -= SetFlameValueText;
    }
    void Update()
    {
        stageTimeText.SetText(FormatHMS(GameManager.Instance.stageTime));
        ingameTimeText.SetText(FormatHMS(GameManager.Instance.ingameTime));
    }

    public string FormatHMS(float seconds)
    {
        if (seconds < 0f) seconds = 0f;

        int total = Mathf.FloorToInt(seconds);
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;

        return $"{h:00}:{m:00}:{s:00}";
    }

    void SetFlameValueText()
    {
        flameNumText.SetText($"{GameManager.Instance.HasFlame}");
    }
}


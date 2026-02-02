using System.Collections;
using TMPro;
using UnityEngine;

public class IngameInfoUI : MonoBehaviour
{
    [SerializeField] TMP_Text stageTimeText;
    [SerializeField] TMP_Text ingameTimeText;
    [SerializeField] TMP_Text stageNumText;
    [SerializeField] TMP_Text flameNumText;

    GameManager GM;
    Coroutine bindCo;
    bool bound;

    void OnEnable()
    {
        if (bound) return;

        if (bindCo == null) bindCo = StartCoroutine(Co_BindGM());
    }

    void OnDisable()
    {
        if (bindCo != null)
        {
            StopCoroutine(bindCo);
            bindCo = null;
        }

        if (bound && GM != null)
        {
            GM.changedHasFlame -= SetFlameValueText;
            bound = false;
        }

        GM = null;
    }

    IEnumerator Co_BindGM()
    {
        while (GameManager.Instance == null)
            yield return null;

        GM = GameManager.Instance;

        if (!bound)
        {
            GM.changedHasFlame += SetFlameValueText;
            bound = true;
        }

        bindCo = null;
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


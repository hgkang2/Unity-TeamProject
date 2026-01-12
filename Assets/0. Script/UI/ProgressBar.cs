using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] Image fillImage;
    [SerializeField] bool useSmooth;
    [SerializeField] float smoothSpeed = 5f;

    float targetFill = 1f;

    public void SetRatio(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        targetFill = ratio;

        if (!useSmooth && fillImage != null)
        {
            fillImage.fillAmount = targetFill;
        }
    }

    public void SetValue(float current, float max)
    {
        if (max <= 0f)
        {
            SetRatio(0f);
            return;
        }

        float ratio = current / max;
        SetRatio(ratio);
    }

    void Update()
    {
        if (!useSmooth || fillImage == null)
        {
            return;
        }

        if (Mathf.Approximately(fillImage.fillAmount, targetFill))
        {
            return;
        }

        float newFill = Mathf.MoveTowards(fillImage.fillAmount, targetFill, smoothSpeed * Time.deltaTime);
        fillImage.fillAmount = newFill;
        
    }
}

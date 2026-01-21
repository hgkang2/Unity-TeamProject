using UnityEngine;
using TMPro;

[RequireComponent(typeof(ProgressBar))]
public class ExpBar : MonoBehaviour
{
    Exp targetExp;
    [SerializeField] TMP_Text curExpText;
    [SerializeField] TMP_Text maxExpText;
    [SerializeField] TMP_Text curLevelText;

    ProgressBar progressBar;

    void Awake()
    {
        progressBar = GetComponent<ProgressBar>();

        SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
        Bind(sceneContext.player.Exp);
    }


    public void Bind(Exp exp)
    {
        // 혹시 이전에 다른 Exp에 물려 있으면 해제
        Clear();

        targetExp = exp;

        // 이벤트 구독
        targetExp.ExpChanged += HandleExpChanged;
        targetExp.LevelChanged += HandleLevelChanged;

        // 초기값 반영
        HandleExpChanged(targetExp.CurExp, targetExp.MaxExp);
    }

    public void Clear()
    {
        if (targetExp == null) return;

        targetExp.ExpChanged -= HandleExpChanged;
        targetExp.LevelChanged -= HandleLevelChanged;
        targetExp = null;
    }

    void HandleExpChanged(int cur, int max)
    {
        progressBar.SetValue(cur, max);
        curExpText?.SetText("{0}", cur);
        maxExpText?.SetText("{0}", max);
    }
    void HandleLevelChanged(int curLevel)
    {
        curLevelText?.SetText($"Lv. {curLevel}");
    }
}

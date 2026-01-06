using UnityEngine;
using TMPro;

[RequireComponent(typeof(ProgressBar))]
public class HPBar : MonoBehaviour
{
    // HP 수동 지정시 자동 바인드. 런타임 지정시 반드시 수동 Bind 하기
    [SerializeField] HP targetHP;
    [SerializeField] TMP_Text curHPText;
    [SerializeField] TMP_Text maxHPText;
    ProgressBar progressBar;


    // Monster등 움직이는 객체 따라 다니게
    public RectTransform rect;
    public Transform anchor;

    void Awake() 
    {
        progressBar = GetComponent<ProgressBar>();

        SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
        Bind(sceneContext.player.HP);
    }


    void OnDisable()
    {
        Clear();
    }

    public void Bind(HP hp)
    {
        targetHP = hp;

        //이벤트 구독
        targetHP.OnHPChanged += HandleHPChanged;
        //초기값 반영
        progressBar.SetValue(targetHP.CurHP, targetHP.MaxHP);
    }

    public void Clear(){
        if (targetHP == null) return;

        targetHP.OnHPChanged -= HandleHPChanged;
        targetHP = null;
    }

    void HandleHPChanged(float cur, float max)
    {
        if (progressBar == null)
        {
            Debug.Log("progressbar null. 채워넣으시오");
            return;
        }

        progressBar.SetValue(cur, max);
        curHPText.SetText("{0}", cur);
        maxHPText.SetText("{0}", max);
    }
}

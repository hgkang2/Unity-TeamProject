using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BossLunaGenesis : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    Vector2 genesisTargetPos;
    GameObject owner;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeGenesis(Vector2 targetPos, float duration, GameObject owner)
    {
        genesisTargetPos = targetPos;
        this.owner = owner;

        StartCoroutine(FadeIn(duration));
    }

    IEnumerator FadeIn(float duration)
    {
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            spriteRenderer.color = c;
            yield return null;
        }

        c.a = 1f;
        spriteRenderer.color = c;
    }
}

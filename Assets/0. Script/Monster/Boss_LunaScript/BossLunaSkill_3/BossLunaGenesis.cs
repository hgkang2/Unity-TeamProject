using System.Collections;
using NUnit.Framework.Constraints;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BossLunaGenesis : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    Vector2 genesisTargetPos;
    public GameObject genesisSpriteMask;
    public GameObject genesisArea;
    public GameObject genesisMainLight;
    public GameObject genesisEffect;
    public Animator genesisAnime;
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

    public void InitializeGenesis(Vector2 targetPos, float speed, GameObject owner)
    {
        genesisTargetPos = targetPos;
        this.owner = owner;

        StartCoroutine(FadeIn(speed));
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
        
        StartCoroutine(GenesisPrepare(genesisTargetPos));
    }

    IEnumerator GenesisPrepare(Vector2 targetPos)
    {
        float speed = 20f;
        while (Vector3.Distance(genesisSpriteMask.transform.position, targetPos) > 0.01f)
        {
            genesisSpriteMask.transform.position =
                Vector3.MoveTowards(genesisSpriteMask.transform.position, targetPos, speed * Time.deltaTime);

            yield return null;
        }

        transform.position = targetPos;

        yield return new WaitForSeconds(0.01f);

        Vector3 scale = genesisSpriteMask.transform.localScale;
        while(scale.x < 10f)
        {
            scale.x += speed * Time.deltaTime;
            genesisSpriteMask.transform.localScale = scale;

            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        genesisMainLight.SetActive(true);
        Vector3 genesisMainLightScale = genesisMainLight.transform.localScale;

        while (genesisMainLightScale.y <= 1f)
        {
            genesisMainLightScale.y += 30f * Time.deltaTime;
            genesisMainLight.transform.localScale = genesisMainLightScale;

            yield return null;
        }

        genesisArea.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        genesisEffect.SetActive(true);

        yield return new WaitForSeconds(1f);

        while (genesisMainLightScale.x > 0f)
        {
            genesisMainLightScale.x -= 20f * Time.deltaTime;
            genesisMainLight.transform.localScale = genesisMainLightScale;

            yield return null;
        }

        genesisMainLight.SetActive(false);
        
        genesisAnime.SetTrigger("genesisOver");

        yield return new WaitForSeconds(0.55f);

        StartCoroutine(FadeOut(1f));
    }

    IEnumerator FadeOut(float duration)
    {
        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            spriteRenderer.color = c;
            yield return null;
        }

        c.a = 0f;
        spriteRenderer.color = c;

        Destroy(this.gameObject);
    }
}
using UnityEngine;
using System;
using System.Collections;

public class BossLunaGenesisAnime : MonoBehaviour
{
    public GameObject spriteMask;

    void Start()
    {
        StartCoroutine(ScaleChangeTest());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ScaleChangeTest()
    {
        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            gameObject.transform.position += new Vector3(0f, -0.1f, 0f);

            yield return null;
        }

        t = 0f;
        yield return new WaitForSeconds(0.01f);

        while (t < 2f)
        {
            t += Time.deltaTime;
            gameObject.transform.localScale += new Vector3(0.05f, 0f, 0.00f);

            yield return null;
        }
    }
}

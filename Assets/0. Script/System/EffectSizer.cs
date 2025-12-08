using UnityEngine;

public class EffectSizer : MonoBehaviour
{
    public float minScale = 0.6f;
    public float maxScale = 1.2f;
    public float delay = 0.05f;

    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= delay)
        {
            timer = 0f;
            float randScale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(randScale, randScale, randScale);
        }
    }
}

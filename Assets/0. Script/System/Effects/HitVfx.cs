using UnityEngine;

public class HitVfx : MonoBehaviour
{
    [System.Serializable]
    public struct Variant
    {
        public GameObject prefab;
        public Vector2 scaleRange;     // min / max
        public Vector2 animSpeedRange; // min / max
    }

    [SerializeField] Variant[] variants;
    [SerializeField] float normalOffset = 0.02f;

    public void Play(in HitContext ctx)
    {
        if (variants == null || variants.Length == 0)
            return;

        int index = Random.Range(0, variants.Length);
        Variant v = variants[index];

        if (v.prefab == null)
            return;

        Vector3 pos = ctx.point + ctx.normal * normalOffset;

        GameObject go = Instantiate(v.prefab, pos, Quaternion.identity);

        // 크기 랜덤
        float scale = Random.Range(v.scaleRange.x, v.scaleRange.y);
        go.transform.localScale = Vector3.one * scale;

        // 애니메이션 속도 랜덤
        Animator anim = go.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            float speed = Random.Range(v.animSpeedRange.x, v.animSpeedRange.y);
            anim.speed = speed;
        }
    }
}

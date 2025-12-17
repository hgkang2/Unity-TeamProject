using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UnbreakableSFX : MonoBehaviour, IHitReceiver
{
    [SerializeField] AudioClip[] hitClips;
    [Range(0f, 1f)]
    [SerializeField] float volume = 1f;

    // 한 프레임에 여러번 효과 재생 방지
    // float lastPlayTime;
    // [SerializeField] float minInterval = 0.05f;

    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D 게임 기준 (필요하면 1f)
    }

    public void OnHit(HitContext ctx)
    {
        // 한 프레임에 여러번 효과 재생 방지
        // if (Time.time - lastPlayTime < minInterval) return;
        
        if (hitClips == null || hitClips.Length == 0)
            return;

        int index = Random.Range(0, hitClips.Length);
        AudioClip clip = hitClips[index];
        if (clip == null)
            return;

        source.PlayOneShot(clip, volume);
    }
}

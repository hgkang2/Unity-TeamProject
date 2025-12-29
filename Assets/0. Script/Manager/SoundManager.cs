using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 런타임 이전에 강제 생성
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // static void AutoCreate()
    // {
    //     if (Instance != null) return;
    //     SoundManager existing = Object.FindAnyObjectByType<SoundManager>();
    //     if (existing != null)
    //     {
    //         Instance = existing;
    //         return;
    //     }
    //     GameObject go = new GameObject("[SoundManager]");
    //     go.AddComponent<SoundManager>();
    // }

    public static SoundManager Instance { get; private set; }

    [Header("Global Sources")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource uiSource;

    [Header("Global Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    readonly Dictionary<string, ClipEntry> bgmLibrary = new();
    readonly Dictionary<string, ClipEntry> uiLibrary = new();

    public event System.Action OnVolumeChanged;


    [System.Serializable]
    public class ClipEntry
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float clipVolume = 1f;
    }

    [SerializeField] List<ClipEntry> bgmClips = new();
    [SerializeField] List<ClipEntry> uiClips = new();

      float currentBgmClipVolume = 1f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLibrary(bgmClips, bgmLibrary);
        BuildLibrary(uiClips, uiLibrary);

        UpdateVolumes();
    }

    void BuildLibrary(List<ClipEntry> list, Dictionary<string, ClipEntry> dict)
    {
        dict.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            ClipEntry entry = list[i];
            if (entry == null) continue;
            if (string.IsNullOrEmpty(entry.key)) continue;
            if (entry.clip == null) continue;

            if (!dict.ContainsKey(entry.key))
            {
                dict.Add(entry.key, entry);
            }
            else
            {
                Debug.LogWarning($"[SoundManager] Duplicate key ignored: {entry.key}");
            }
        }
    }
    // SoundManager에 등록된 클립을 사용
    public void PlayBGM(string key, bool loop = true)
    {
        if (!bgmLibrary.TryGetValue(key, out ClipEntry entry) || entry.clip == null)
        {
            Debug.LogWarning($"[SoundManager] BGM key not found: {key}");
            return;
        }

        PlayBGM(entry.clip, entry.clipVolume, loop);
    }

    // 개별 클립 사용
    public void PlayBGM(AudioClip clip, float clipVolume = 1f, bool loop = true)
    {
        if (clip == null) return;

        currentBgmClipVolume = Mathf.Clamp01(clipVolume);

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        UpdateVolumes();
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }


    public void PlayUI(string key)
    {
        if (!uiLibrary.TryGetValue(key, out ClipEntry entry) || entry.clip == null)
        {
            Debug.LogWarning($"[SoundManager] UI key not found: {key}");
            return;
        }

        PlayUI(entry.clip, entry.clipVolume);
    }

    public void PlayUI(AudioClip clip, float clipVolume = 1f)
    {
        if (clip == null) return;

        float v = masterVolume * sfxVolume * Mathf.Clamp01(clipVolume);
        uiSource.PlayOneShot(clip, v);
    }


    void UpdateVolumes()
    {
        bgmSource.volume = masterVolume * bgmVolume * currentBgmClipVolume;
        // SFX는 PlayOneShot 때 계산해서 적용
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        OnVolumeChanged?.Invoke();
    }

    public float GetGlobalSfxVolume()
    {
        return masterVolume * sfxVolume;
    }
}

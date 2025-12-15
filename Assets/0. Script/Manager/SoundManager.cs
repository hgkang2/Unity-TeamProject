using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Global Sources")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource uiSource;

    [Header("Global Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    Dictionary<string, AudioClip> bgmLibrary = new();
    Dictionary<string, AudioClip> uiLibrary = new();

    public event System.Action OnVolumeChanged;


    [System.Serializable]
    public class NamedClip
    {
        public string key;
        public AudioClip clip;
    }

    [SerializeField] List<NamedClip> bgmClips = new();
    [SerializeField] List<NamedClip> uiClips = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (NamedClip pair in bgmClips)
        {
            if (pair.clip != null && !bgmLibrary.ContainsKey(pair.key))
                bgmLibrary.Add(pair.key, pair.clip);
        }

        foreach (NamedClip pair in uiClips)
        {
            if (pair.clip != null && !uiLibrary.ContainsKey(pair.key))
                uiLibrary.Add(pair.key, pair.clip);
        }
    }


    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = masterVolume * bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }


    public void PlayUI(AudioClip clip)
    {
        if (clip == null) return;
        uiSource.PlayOneShot(clip, masterVolume);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        OnVolumeChanged?.Invoke();
    }

    void UpdateVolumes()
    {
        bgmSource.volume = masterVolume * bgmVolume;
        // SFX는 각 LocalSoundVFX가 개별적으로 적용
    }
    public float GetGlobalSfxVolume()
    {
        return masterVolume * sfxVolume;
    }
}

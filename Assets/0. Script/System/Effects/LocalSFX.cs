using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LocalSFX : MonoBehaviour
{
    [Serializable]
    public class SoundEntry
    {
        [Tooltip("Play() 호출 시 사용할 키 이름")]
        public string key;

        [Tooltip("이 키에서 무작위로 재생될 클립들")]
        public List<AudioClip> clips = new();

        [Range(0f, 1f)]
        public float volume = 1f;

    }

    [Header("사운드 목록")]
    public List<SoundEntry> sounds = new();

    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    AudioSource source;
    readonly Dictionary<string, AudioSource> loopSources = new();

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = 20f;
    }

    void OnEnable()
    {
        if(SoundManager.Instance) SoundManager.Instance.OnVolumeChanged += HandleVolumeChanged;
    }

    void OnDisable()
    {
        if(SoundManager.Instance) SoundManager.Instance.OnVolumeChanged -= HandleVolumeChanged;
        StopAllLoops();
    }

    // 단발음 재생
    public void Play(string key)
    {
        SoundEntry entry = FindEntry(key);
        if (entry == null || entry.clips.Count == 0) return;

        AudioClip clip = GetRandomClip(entry);
        if (clip == null) return;

        float volume = entry.volume * GetGlobalSfxVolume();
        source.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
        source.PlayOneShot(clip, volume);
    }

    // 지속음 재생 (loop). 끝날 때 StopLoop 해줘야 함.
    public void PlayLoop(string key)
    {
        SoundEntry entry = FindEntry(key);
        if (entry == null || entry.clips.Count == 0) return;
        if (loopSources.ContainsKey(key)) return; // 이미 재생 중이면 무시

        AudioClip clip = GetRandomClip(entry);
        if (clip == null) return;

        AudioSource loopSrc = gameObject.AddComponent<AudioSource>();
        loopSrc.clip = clip;
        loopSrc.loop = true;
        loopSrc.playOnAwake = false;
        loopSrc.spatialBlend = 1f;
        loopSrc.volume = entry.volume * GetGlobalSfxVolume();
        loopSrc.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
        loopSrc.Play();

        loopSources[key] = loopSrc;
    }

    public void StopLoop(string key)
    {
        if (loopSources.TryGetValue(key, out AudioSource loopSrc))
        {
            loopSrc.Stop();
            Destroy(loopSrc);
            loopSources.Remove(key);
        }
    }

    void StopAllLoops()
    {
        var keys = new List<string>(loopSources.Keys);

        foreach (var key in keys)
            StopLoop(key);

        loopSources.Clear();
    }

    void HandleVolumeChanged()
    {
        float global = GetGlobalSfxVolume();

        foreach (KeyValuePair<string, AudioSource> pair in loopSources)
        {
            string key = pair.Key;
            AudioSource loopSrc = pair.Value;

            SoundEntry entry = FindEntry(key);
            if (entry != null && loopSrc != null)
            {
                loopSrc.volume = entry.volume * global;
            }
        }
    }

    // 내부 유틸
    SoundEntry FindEntry(string key)
    {
        return sounds.Find(s => s != null && s.key == key);
    }

    AudioClip GetRandomClip(SoundEntry e)
    {
        List<AudioClip> valid = e.clips.FindAll(c => c != null);
        if (valid.Count == 0) return null;
        return valid[UnityEngine.Random.Range(0, valid.Count)];
    }

    float GetGlobalSfxVolume()
    {
        if (SoundManager.Instance == null) return 1f;
        return SoundManager.Instance.masterVolume * SoundManager.Instance.sfxVolume;
    }
}

using System;
using UnityEngine;

public static class SettingsManager
{
    struct SettingsData
    {
        public float bgm01; // 0~1
        public float sfx01; // 0~1
        public int resW;
        public int resH;
    }

    static SettingsData committed;
    static SettingsData working;

    public static bool IsDirty { get; private set; }
    public static event Action<bool> DirtyChanged;

    const string KEY_HAS   = "SETTINGS_HAS";
    const string KEY_BGM   = "SETTINGS_BGM01";
    const string KEY_SFX   = "SETTINGS_SFX01";
    const string KEY_RES_W = "SETTINGS_RES_W";
    const string KEY_RES_H = "SETTINGS_RES_H";

    public static bool CanApply => IsDirty;

    public static void LoadOrDefault()
    {
        if (PlayerPrefs.GetInt(KEY_HAS, 0) == 1)
        {
            committed = new SettingsData
            {
                bgm01 = Mathf.Clamp01(PlayerPrefs.GetFloat(KEY_BGM, 1f)),
                sfx01 = Mathf.Clamp01(PlayerPrefs.GetFloat(KEY_SFX, 1f)),
                resW  = PlayerPrefs.GetInt(KEY_RES_W, 0),
                resH  = PlayerPrefs.GetInt(KEY_RES_H, 0),
            };

            if (committed.resW <= 0 || committed.resH <= 0)
                GetMaxResolution(out committed.resW, out committed.resH);
        }
        else
        {
            committed = new SettingsData { bgm01 = 1f, sfx01 = 1f };
            GetMaxResolution(out committed.resW, out committed.resH);
        }

        working = committed;
        ApplyWorkingToRuntime();
        SetDirty(false);
    }

    public static void CommitAndSave()
    {
        committed = working;

        PlayerPrefs.SetInt(KEY_HAS, 1);
        PlayerPrefs.SetFloat(KEY_BGM, committed.bgm01);
        PlayerPrefs.SetFloat(KEY_SFX, committed.sfx01);
        PlayerPrefs.SetInt(KEY_RES_W, committed.resW);
        PlayerPrefs.SetInt(KEY_RES_H, committed.resH);
        PlayerPrefs.Save();

        SetDirty(false);
    }

    public static void RevertWorkingToCommitted()
    {
        working = committed;
        ApplyWorkingToRuntime();
        SetDirty(false);
    }

    public static void SetWorkingBgm(float v01)
    {
        working.bgm01 = Mathf.Clamp01(v01);
        ApplyAudioPreview();
        UpdateDirty();
    }

    public static void SetWorkingSfx(float v01)
    {
        working.sfx01 = Mathf.Clamp01(v01);
        ApplyAudioPreview();
        UpdateDirty();
    }

    public static void SetWorkingResolution(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        working.resW = width;
        working.resH = height;

        ApplyResolutionPreview();
        UpdateDirty();
    }

    // ---------------- internal ----------------
    static void UpdateDirty()
    {
        bool dirty =
            !Mathf.Approximately(working.bgm01, committed.bgm01) ||
            !Mathf.Approximately(working.sfx01, committed.sfx01) ||
            working.resW != committed.resW ||
            working.resH != committed.resH;

        SetDirty(dirty);
    }

    static void SetDirty(bool dirty)
    {
        if (IsDirty == dirty) return;
        IsDirty = dirty;
        DirtyChanged?.Invoke(IsDirty);
    }

    static void ApplyWorkingToRuntime()
    {
        ApplyAudioPreview();
        ApplyResolutionPreview();
    }

    static void ApplyAudioPreview()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.SetBgmVolume(working.bgm01);
        SoundManager.Instance.SetSfxVolume(working.sfx01);
    }

    static void ApplyResolutionPreview()
    {
        if (working.resW <= 0 || working.resH <= 0) return;
        Screen.SetResolution(working.resW, working.resH, Screen.fullScreen);
    }

    static void GetMaxResolution(out int w, out int h)
    {
        w = Screen.width;
        h = Screen.height;

        var rs = Screen.resolutions;
        if (rs == null || rs.Length == 0) return;

        Resolution best = rs[0];
        for (int i = 1; i < rs.Length; i++)
        {
            var r = rs[i];
            if (r.width > best.width) best = r;
            else if (r.width == best.width && r.height > best.height) best = r;
        }

        w = best.width;
        h = best.height;
    }
}

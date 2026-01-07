using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VFX/Keyed Attack VFX Library")]
public class AttackVFXLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string key;
        public GameObject prefab;
        public bool followOwner;

        public Vector3 localOffset;
        public Vector3 localEuler;
    }

    [SerializeField] Entry[] entries;

    Dictionary<string, Entry> cache;

    void OnEnable()
    {
        cache = new Dictionary<string, Entry>(entries.Length);

        for (int i = 0; i < entries.Length; i++)
        {
            Entry e = entries[i];
            if (string.IsNullOrEmpty(e.key))
            {
                Debug.LogWarning($"key가 널임: {e.key}");
                continue;
            }
            if (cache.ContainsKey(e.key))
            {
                Debug.LogWarning($"키가 중복됨: {e.key}");
                continue;
            }

            cache.Add(e.key, e);
        }
    }

    public Entry Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        Entry e;
        if (cache != null && cache.TryGetValue(key, out e))
        {
            return e;
        }

        Debug.LogWarning($"키가 없어요: {key}");
        return null;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IngameFollowBGSet
{
    public string key;
    public GameObject root;
}

public class InGameFollowBGManager : MonoBehaviour
{
    public static InGameFollowBGManager Instance { get; private set; }

    [SerializeField] IngameFollowBGSet[] sets;
    [SerializeField] string startKey;

    Dictionary<string, GameObject> dict;
    GameObject currentRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildDictionary();
        DisableAll();

        if (!string.IsNullOrEmpty(startKey))
            ChangeIngameBG(startKey);
    }

    void BuildDictionary()
    {
        dict = new Dictionary<string, GameObject>(StringComparer.Ordinal);

        foreach (var s in sets)
        {
            if (string.IsNullOrEmpty(s.key) || s.root == null)
                continue;

            if (dict.ContainsKey(s.key))
            {
                Debug.LogWarning($"Duplicate BG key: {s.key}", this);
                continue;
            }

            dict.Add(s.key, s.root);
        }
    }

    void DisableAll()
    {
        foreach (var r in dict.Values)
            r.SetActive(false);
    }

    public bool ChangeIngameBG(string key)
    {
        if (!dict.TryGetValue(key, out var nextRoot))
        {
            Debug.LogWarning($"BG key not found: {key}", this);
            return false;
        }

        if (currentRoot == nextRoot)
            return true;

        if (currentRoot != null)
            currentRoot.SetActive(false);

        currentRoot = nextRoot;
        currentRoot.SetActive(true);

        return true;
    }
}

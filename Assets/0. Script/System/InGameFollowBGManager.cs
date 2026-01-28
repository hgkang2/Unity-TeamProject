using System;
using System.Collections.Generic;
using UnityEngine;

public enum BGSetKey
{
    Ground,
    UnderGround
}

[Serializable]
public class IngameFollowBGSet
{
    public BGSetKey key;
    public GameObject root;
}
public class InGameFollowBGManager : MonoBehaviour
{
    public static InGameFollowBGManager Instance { get; private set; }

    [SerializeField] IngameFollowBGSet[] sets;
    [SerializeField] BGSetKey startKey;

    Dictionary<BGSetKey, GameObject> dict;
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

        ChangeIngameBG(startKey);
    }

    void BuildDictionary()
    {
        dict = new Dictionary<BGSetKey, GameObject>();

        foreach (var s in sets)
        {
            if (s.root == null)
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

    public bool ChangeIngameBG(BGSetKey key)
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
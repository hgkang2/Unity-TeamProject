using UnityEngine;
using System;
using System.Collections;

public class BossLunaGenesisAnime : MonoBehaviour
{
    void OnGenesisOver()
    {
        Destroy(this.gameObject, 0.5f);
    }
}

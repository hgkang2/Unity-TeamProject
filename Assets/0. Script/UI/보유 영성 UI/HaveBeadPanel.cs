using System.Collections.Generic;
using UnityEngine;

public class HaveBeadPanel : MonoBehaviour
{
    [Header("영단 고정된 순서대로 지정(atk, heal, move, walljump)")]
    [SerializeField] SoulData[] beadData;

    [Header("hierarchy 영단슬롯 끌어넣기")]
    [SerializeField] HaveBeadSlot[] beadSlots;

    void Awake()
    {
        if(beadData.Length != beadSlots.Length)
        {
            Debug.LogWarning($"HaveBeadPanel 사전 설정 오류(길이 맞추기)");
            return;
        }

        //슬롯별로 고정된 영단 슬롯 지정
        for(int i=0; i<beadData.Length; i++)
        {
            beadSlots[i].PreSet(beadData[i]);
        }
    }

    public void Set(IEnumerable<SoulInstance> items)
    {
        foreach(var slot in beadSlots)
        {
            slot.gameObject.SetActive(false);
        }

        foreach(var item in items)
        {
            foreach(var slot in beadSlots)
            {
                if(item.data == slot.SoulData)
                {
                    slot.Set(item);
                    slot.gameObject.SetActive(true);
                }
            }
        }
    }
}

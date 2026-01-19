using System.Collections.Generic;
using UnityEngine;

public class HaveStatPanel : MonoBehaviour
{
    List<HaveStatSlot> slots = new List<HaveStatSlot>();
    [SerializeField] Transform slotParent;
    [SerializeField] HaveStatSlot statSlotPrefab;
    public void AddStat(SoulInstance instance)
    {
        // 1. 꺼져있는 슬롯이 있다면 재사용
        HaveStatSlot targetSlot = slots.Find(s => !s.gameObject.activeSelf);

        // 2. 없다면 새로 생성
        if (targetSlot == null)
        {
            targetSlot = Instantiate(statSlotPrefab, slotParent);
            slots.Add(targetSlot);
        }

        // 3. 데이터 세팅 및 활성화
        // ex) {ATK +} {5 *stack} {%}
        targetSlot.statText.SetText($"{instance.data.soulEffectText}{instance.data.GetValue() * instance.stack}{instance.data.soulEffectText2}");
        targetSlot.gameObject.SetActive(true);
    }

    public void Clear()
    {
        foreach (var slot in slots)
        {
            slot.gameObject.SetActive(false);
        }
    }
}

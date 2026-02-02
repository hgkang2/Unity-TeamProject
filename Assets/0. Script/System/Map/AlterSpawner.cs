using UnityEngine;
using System.Collections.Generic;

public class AlterSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject alterPrefab; // 생성할 제단 프리팹
    public Transform[] spawnPoints; // 스폰 지점들의 배열

    void Start()
    {
        SpawnAlter();
    }

    public void SpawnAlter()
    {
        if (spawnPoints.Length == 0 || alterPrefab == null)
        {
            Debug.LogWarning("스폰 지점이나 제단 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 1. 랜덤하게 인덱스 선택
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];

        // 2. 선택된 위치와 회전값으로 제단 생성
        Instantiate(alterPrefab, selectedPoint.position, Quaternion.identity);
        
        // (선택 사항) 어디에 생성되었는지 로그 확인
        Debug.Log($"{selectedPoint.name} 위치에 제단이 생성되었습니다.");
    }
}
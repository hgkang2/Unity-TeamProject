using UnityEngine;
using System.Collections.Generic;

public class CrystalSpawner : MonoBehaviour
{
    [Header("크리스탈 종류 (여러 개를 드래그해서 넣으세요)")]
    public GameObject[] crystalPrefabs; // 크리스탈 프리팹들을 담는 배열

    [Header("스폰 포인트 위치들")]
    public Transform[] spawnPoints; // 스폰 지점들을 담는 배열

    public int spawnCount = 3; // 생성하고 싶은 크리스탈 개수

void Start()
{
    SpawnMultipleRandomly();
}

public void SpawnMultipleRandomly()
    {
        // 포인트 개수보다 생성 요청 개수가 많으면 안 되므로 방어 코드 추가
        int actualCount = Mathf.Min(spawnCount, spawnPoints.Length);

        // 포인트 리스트를 복사해서 섞기 (중복 위치 방지)
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

    for (int i = 0; i < actualCount; i++)
        {
            // 남은 포인트 중 랜덤 하나 선택
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform targetPoint = availablePoints[randomIndex];

            // 랜덤 크리스탈 선택
            int crystalIndex = Random.Range(0, crystalPrefabs.Length);

            // 생성
            Instantiate(crystalPrefabs[crystalIndex], targetPoint.position, targetPoint.rotation);

            // 한 곳에 두 번 생성되지 않도록 목록에서 제거
            availablePoints.RemoveAt(randomIndex);
        }
    }
}
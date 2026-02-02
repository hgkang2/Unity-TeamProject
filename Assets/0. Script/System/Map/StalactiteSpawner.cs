using UnityEngine;

public class StalactiteSpawner : MonoBehaviour
{
    public GameObject stalactitePrefab; // 종유석 프리팹
    public float spawnInterval = 5.0f;  // 생성 간격 (5초마다)

    void Start()
    {
        // spawnInterval마다 SpawnStalactite 함수를 반복 호출
        InvokeRepeating("SpawnStalactite", 0f, spawnInterval);
    }

    void SpawnStalactite()
    {
        // 현재 스패너 위치에 종유석 생성
        Instantiate(stalactitePrefab, transform.position, Quaternion.identity);
    }
}
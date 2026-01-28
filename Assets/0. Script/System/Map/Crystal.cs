using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour, IDamageable
{
    HP hp;
    Animator anim;
    Collider2D col;
    LocalSFX sfx;

    public List<GameObject> itemPrefabs;

    [Header("드랍 설정")]
    [SerializeField] private int minDropCount = 3; // 인스펙터에서 최소 개수 설정
    [SerializeField] private int maxDropCount = 3; // 인스펙터에서 최대 개수 설정
    [SerializeField] private float scatterForceMin = 2f;
    [SerializeField] private float scatterForceMax = 4f;

    private bool isBroken = false;

    void Awake()
    {
        hp = GetComponent<HP>();
        hp.OnDied += Die;
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        sfx = GetComponent<LocalSFX>();
    }

    // 인터페이스 구현 (IDamageable이 정의되어 있어야 함)
    // 주의: IDamageable 인터페이스를 쓰려면 'public class Crystal : MonoBehaviour, IDamageable' 형식이어야 합니다.
    public void TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        hp.TakeDamage(amount);
    }

    void Die()
    {
        if (isBroken) return;
        isBroken = true;

        DropItems();
        if (col != null) col.enabled = false;
        if (anim != null) anim.SetTrigger("OnDestroy");
        if (sfx != null) sfx.Play("OnDestroy");
        Destroy(gameObject, 0.1f);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void DropItems()
    {
        if (itemPrefabs == null || itemPrefabs.Count == 0) return;

        // 1. 종류 결정 (랜덤하게 1종류만 선택)
        int randomIndex = Random.Range(0, itemPrefabs.Count);
        GameObject selectedItem = itemPrefabs[randomIndex];

        Vector2 dropPosition = (Vector2)transform.position + new Vector2(0, 0.5f);

        // 2. 개수 결정 (인스펙터에서 설정한 값 사용)
        // Random.Range(int, int)는 마지막 숫자를 포함하지 않으므로 +1을 해줍니다.
        int dropAmount = Random.Range(minDropCount, maxDropCount + 1);

        for (int i = 0; i < dropAmount; i++)
        {
            GameObject droppedItem = Instantiate(selectedItem, dropPosition, Quaternion.identity);
            Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                // 방향: 좌우 랜덤, 위쪽 방향 고정(0.5~1.0)
                Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
                float randomForce = Random.Range(scatterForceMin, scatterForceMax);
                
                rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-0.5f, 0.5f), ForceMode2D.Impulse);
            }
        }
    }
}
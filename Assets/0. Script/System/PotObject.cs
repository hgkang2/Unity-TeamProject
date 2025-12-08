using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PotObject : MonoBehaviour, IDamageable
{
    HP hp ;
    public List<GameObject> itemPrefabs;
    public float dropForce = 5f;

    private bool isBroken = false;

    void Awake()
    {
         hp = GetComponent<HP>();
         if (hp == null)
        {
            enabled = false;
            return;
        }
        hp.OnDied += OnDie;
    }
    public void TakeDamage(float amount)
    {
        hp.TakeDamage(amount);
    }

    public void TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        //비우기
    }


    //부숴졌을 때(hp가 0이 되었을때) 구슬 랜덤으로 뱉기

    void OnDie()
    {
        if(isBroken) return;
        isBroken = true;
        DropItems();
        Destroy(gameObject);
    }

    private void DropItems()
    {
        if(itemPrefabs.Count == 0)
        {
            return;
        }
        int randomIndex = Random.Range(0,itemPrefabs.Count);
        GameObject selectedItem = itemPrefabs[randomIndex];
        Vector2 dropPosition = transform.position + new Vector3(0,0.5f,0);
        int ItemsDrop = Random.Range(1,6);
        for (int i = 0; i < ItemsDrop; i++)
        {
            GameObject droppedItem = Instantiate(selectedItem,dropPosition,Quaternion.identity);
            Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                Vector2 randomDirection = new Vector2
                (
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1f)
                ).normalized;
                rb.AddForce(randomDirection * dropForce,ForceMode2D.Impulse);
            }
        }
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

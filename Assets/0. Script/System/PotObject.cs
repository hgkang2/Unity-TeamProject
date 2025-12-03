using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PotObject : MonoBehaviour, IDamageable
{
    HP hp ;
    public List<GameObject> itemPrefabs;
    public int numberOfItemsDrop = 1;
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
        for (int i = 0; i < numberOfItemsDrop; i++)
        {
            int randomIndex = Random.Range(0, itemPrefabs.Count);
            GameObject selectedItem = itemPrefabs[randomIndex];
            GameObject droppedItem = Instantiate(selectedItem,transform.position,Quaternion.identity);
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if(rb != null)
            {
                Vector3 randomDirection = new Vector3
                (
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(-1f,1f)
                ).normalized;
                rb.AddForce(randomDirection * dropForce,ForceMode.Impulse);
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

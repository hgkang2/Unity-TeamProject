using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PotObject : MonoBehaviour, IDamageable
{
    HP hp;
    Animator anim;
    Collider2D col;
    LocalSFX sfx;

    public List<GameObject> itemPrefabs;
    public int dropMinAmount;
    public int dropMaxAmount;

    private bool isBroken = false;

    void Awake()
    {
        hp = GetComponent<HP>();
        hp.OnDied += Die;
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        sfx = GetComponent<LocalSFX>();
    }

    void IDamageable.TakeDamage(float amount, DamageType type, Vector2? attackerWorldPosition)
    {
        hp.TakeDamage(amount);
    }


    //부숴졌을 때(hp가 0이 되었을때) 구슬 랜덤으로 뱉기

    void Die()
    {
        if (isBroken) return;
        isBroken = true;


        DropItems();
        col.enabled = false;
        anim.SetTrigger("OnDestroy");
        sfx.Play("OnDestroy");
    }
    // Animation Clip 마지막 프레임에서 호출
    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void DropItems()
    {
        if (itemPrefabs.Count == 0)
        {
            return;
        }
        int randomIndex = Random.Range(0, itemPrefabs.Count);
        GameObject selectedItem = itemPrefabs[randomIndex];
        Vector2 dropPosition = transform.position + new Vector3(0, 0.5f, 0);
        int ItemsDrop = Random.Range(dropMinAmount, dropMaxAmount);
        for (int i = 0; i < ItemsDrop; i++)
        {
            GameObject droppedItem = Instantiate(selectedItem, dropPosition, Quaternion.identity);
            Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDirection = new Vector2
                (
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1f)
                );

                float randomForce = Random.Range(2f, 4f);
                rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);

                float randomTorque = Random.Range(-0.5f, 0.5f);
                rb.AddTorque(randomTorque, ForceMode2D.Impulse);
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

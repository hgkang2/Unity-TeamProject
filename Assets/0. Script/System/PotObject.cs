using UnityEngine;

public class PotObject : MonoBehaviour, IDamageable
{
    HP hp;

    public void TakeDamage(float amount)
    {
        hp.TakeDamage(amount);
    }

    public void TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        //비우기
    }


    //부숴졌을 때(hp가 0이 되었을때) 구슬 랜덤으로 뱉기

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

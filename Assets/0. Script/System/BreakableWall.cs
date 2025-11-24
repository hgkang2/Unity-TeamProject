using UnityEngine;
using DG.Tweening;

public class BreakableWall : MonoBehaviour, IDamageable
{
    public float wallHP;

    public void TakeDamage(float amount)
    {
        
        wallHP -= amount;
        transform.DOShakePosition(0.5f, amount*0.01f);
        if(wallHP <= 0) Destroy(gameObject);
    }
}

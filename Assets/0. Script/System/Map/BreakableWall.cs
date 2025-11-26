using UnityEngine;
using DG.Tweening;

public class BreakableWall : MonoBehaviour, IDamageable
{
    public float wallHP;

    public void TakeDamage(float amount)
    {
        
        wallHP -= amount;

        float shakeAmount = Mathf.Log(amount + 1) * 0.025f; 
        transform.DOShakePosition(duration: 0.5f, 
            strength: new Vector3(shakeAmount, 0f, 0f), 
            vibrato: 10, 
            randomness: 90f, 
            fadeOut: true);
        if(wallHP <= 0) Destroy(gameObject);
    }

    public void TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        TakeDamage(amount);
    }
}

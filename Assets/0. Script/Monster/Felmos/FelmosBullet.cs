using UnityEngine;

public class FelmosBullet : MonoBehaviour, IDamageable
{
    [SerializeField]
    Transform PlayerPos;
    float speed;

    Collider2D cd;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void TakeDamage(float amount)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float amount, Vector2 attackerWorldPosition)
    {
        throw new System.NotImplementedException();
    }

}

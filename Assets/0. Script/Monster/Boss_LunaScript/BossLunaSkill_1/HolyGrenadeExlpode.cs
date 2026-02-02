using UnityEngine;

public class HolyGrenadeExplode : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        //if(gameObject) gameObject.SetActive(false);
        animator = GetComponent<Animator>();
    }

    void OnExplodeEnd()
    {
        Destroy(this.gameObject);
    }
}

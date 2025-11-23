using UnityEngine;

public class Item : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other)
    {
        Player p = other.gameObject.GetComponent<Player>();
        if (p != null)
        {
            Debug.Log("아이템 먹음");
            Destroy(this.gameObject);
            //아이템 획득시 할것
        }
    }
}

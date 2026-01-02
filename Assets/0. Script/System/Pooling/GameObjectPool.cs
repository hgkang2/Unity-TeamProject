using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    GameObject prefab;
    Transform root;
    Queue<GameObject> pool;

    public GameObjectPool(GameObject prefab, Transform root, int prewarmCount)
    {
        this.prefab = prefab;
        this.root = root;
        pool = new Queue<GameObject>(prewarmCount);

        for (int i = 0; i < prewarmCount; i++)
        {
            GameObject go = Create();
            Return(go);
        }
    }

    GameObject Create()
    {
        GameObject go = Object.Instantiate(prefab, root);
        go.SetActive(false);

        PooledAttackSpriteVFX vfx = go.GetComponent<PooledAttackSpriteVFX>();
        if (vfx != null)
        {
            vfx.SetPool(this);
        }

        return go;
    }

    public GameObject Rent()
    {
        if (pool.Count > 0)
        {
            GameObject go = pool.Dequeue();

            PooledAttackSpriteVFX vfx = go.GetComponent<PooledAttackSpriteVFX>();
            if (vfx != null)
            {
                vfx.SetPool(this);
            }

            return go;
        }

        return Create();
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(root, false);
        pool.Enqueue(go);
    }
}

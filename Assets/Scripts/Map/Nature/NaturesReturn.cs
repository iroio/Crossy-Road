using UnityEngine;

public class NaturesReturn : MonoBehaviour
{
    GameObjectPool<Transform> _pool;

    public void SetPool(GameObjectPool<Transform> pool)
    {
        _pool = pool;
    }

    public void ReturnNature()
    {
        gameObject.SetActive(false);
        transform.rotation = Quaternion.identity;

        _pool.Set(transform);
    }
}

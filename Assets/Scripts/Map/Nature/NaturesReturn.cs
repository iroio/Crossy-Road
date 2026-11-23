using UnityEngine;

public class NaturesReturn : MonoBehaviour, IPoolable
{
    private GameObjectPool<NaturesReturn> _pool;

    public void SetPool(GameObjectPool<NaturesReturn> pool)
    {
        _pool = pool;
    }

    public void ReturnToPool()
    {
        if (_pool == null)
        {
            Debug.LogError("Nature object has no registered pool.", this);
            return;
        }

        _pool.Return(this);
    }

    // =========================================================
    // Ipoolable ±¸Çö
    // =========================================================
    public void OnCreate() { }
    public void OnGet() { }
    public void OnReturn() 
    {
        gameObject.SetActive(false);
        transform.rotation = Quaternion.identity;
    }
}

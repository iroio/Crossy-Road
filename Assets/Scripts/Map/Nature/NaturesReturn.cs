using UnityEngine;

public class NaturesReturn : MonoBehaviour, IPoolable
{
    private GameObjectPool<NaturesReturn> _pool;

    public void SetPool(GameObjectPool<NaturesReturn> pool)
    {
        _pool = pool;
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

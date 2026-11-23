using System;
using System.Collections.Generic;
using UnityEngine;

// 모든 GameObjectPool을 중앙에서 관리
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private Dictionary<string, object> _pools = new Dictionary<string, object>();

    private readonly Dictionary<Component, Dictionary<string, object>> _ownedPools =
        new Dictionary<Component, Dictionary<string, object>>();

    // 풀 등록
    public void RegisterPool<T>(String  poolName, GameObjectPool<T> pool)
        where T : Component, IPoolable
    {
        if(_pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool '{poolName}' is already registered. Overwriting.");
        }
        _pools[poolName] = pool;
    }

    // 풀 조회
    public GameObjectPool<T> GetPool<T>(string poolName)
        where T : Component, IPoolable
    {
        if( _pools.TryGetValue(poolName, out var pool))
        {
            if (pool is GameObjectPool<T> typedPool)
                return typedPool;

            Debug.LogError($"Pool '{poolName}' has a different type.");
            return null;
        }
        Debug.LogError($"Pool '{poolName}' not found.");
        return null;
    }

    public void RegisterPool<T>(Component owner, string poolName, GameObjectPool<T> pool)
        where T : Component, IPoolable
    {
        // fix With AI
        if (owner == null || string.IsNullOrEmpty(poolName) || pool == null)
            throw new ArgumentException("Pool owner, name and pool must be valid.");

        if (!_ownedPools.TryGetValue(owner, out var pools))
        {
            pools = new Dictionary<string, object>();
            _ownedPools.Add(owner, pools);
        }

        if (pools.ContainsKey(poolName))
            throw new InvalidOperationException($"Pool '{poolName}' is already registered for '{owner.name}'.");

        pools.Add(poolName, pool);
    }

    public GameObjectPool<T> GetPool<T>(Component owner, string poolName)
        where T : Component, IPoolable
    {
        // fix With AI
        if (owner == null || string.IsNullOrEmpty(poolName))
            throw new ArgumentException("Pool owner and name must be valid.");

        if (!_ownedPools.TryGetValue(owner, out var pools) || !pools.TryGetValue(poolName, out var pool))
            throw new InvalidOperationException($"Pool '{poolName}' is not registered for '{owner.name}'.");

        if (pool is GameObjectPool<T> typedPool)
            return typedPool;

        // fix With AI
        throw new InvalidOperationException($"Pool '{poolName}' has a different type for '{owner.name}'.");
    }

    public void UnregisterPools(Component owner)
    {
        if (!ReferenceEquals(owner, null))
            _ownedPools.Remove(owner);
    }

    // 풀 통계
    public void PrintPoolStatistics()
    {
        Debug.Log("=== Pool Statistics ===");
        foreach (var pair in _pools)
        {
            Debug.Log($"{pair.Key}: {pair.Value.GetType().Name}");
        }

        // fix With AI
        foreach (var owner in _ownedPools)
        {
            foreach (var pool in owner.Value)
                Debug.Log($"{owner.Key.name}/{pool.Key}: {pool.Value.GetType().Name}");
        }
    }
}

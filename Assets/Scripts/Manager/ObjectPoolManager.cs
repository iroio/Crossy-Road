using System;
using System.Collections.Generic;
using UnityEngine;

// 모든 GameObjectPool을 중앙에서 관리
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private Dictionary<string, object> _pools = new Dictionary<string, object>();

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
            return pool as GameObjectPool<T>;
        }
        Debug.LogError($"Pool '{poolName}' not found.");
        return null;
    }

    // 풀 통계
    public void PrintPoolStatistics()
    {
        Debug.Log("=== Pool Statistics ===");
        foreach (var pair in _pools)
        {
            Debug.Log($"{pair.Key}: {pair.Value.GetType().Name}");
        }
    }
}

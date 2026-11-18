using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool<T> where T : Component
{
    readonly T _prefab;
    readonly Transform _poolRoot;
    readonly Action<T> _onCreate;

    readonly Queue<T> _pool = new Queue<T>();
    readonly HashSet<T> _activeObjects = new HashSet<T>();

    public int Available => _pool.Count;
    public int ActiveCount => _activeObjects.Count;

    public GameObjectPool(T prefab, Transform poolRoot, int initialSize, Action<T> onCreate = null)
    {
        _prefab = prefab
            ?? throw new ArgumentNullException(nameof(prefab));

        _poolRoot = poolRoot
            ?? throw new ArgumentNullException(nameof(poolRoot));

        _onCreate = onCreate;

        Initialize(initialSize);
    }

    private void Initialize(int size)
    {
        for (int i = 0; i < size; i++)
        {
            CreateObject();
        }
    }

    private T CreateObject()
    {
        T obj = UnityEngine.Object.Instantiate(_prefab, _poolRoot);
        _onCreate?.Invoke(obj);
        obj.gameObject.SetActive(false);

        _pool.Enqueue(obj);
        return obj;
    }

    public T Get()
    {
        T obj;
        
        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            obj = CreateObject();
        }

        _activeObjects.Add(obj);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (obj == null || !_activeObjects.Remove(obj))
            return;

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_poolRoot);
        obj.transform.rotation = Quaternion.identity;

        _pool.Enqueue(obj);
    }
}
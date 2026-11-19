using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool<T> where T : Component, IPoolable
{
    readonly Transform _poolRoot;
    readonly Func<T> _createFunction;
    readonly Action<T> _onCreate;

    readonly Queue<T> _pool = new Queue<T>();
    readonly HashSet<T> _activeObjects = new HashSet<T>();

    public int Available => _pool.Count;
    public int ActiveCount => _activeObjects.Count;

    public GameObjectPool(Transform poolRoot, int initialSize, 
        Func<T> createFunction, Action<T> onCreate = null)
    {
        _poolRoot = poolRoot
            ?? throw new ArgumentNullException(nameof(poolRoot));

        _createFunction = createFunction
            ?? throw new ArgumentNullException(nameof(createFunction));

        _onCreate = onCreate;

        Initialize(initialSize);
    }

    private void Initialize(int size)
    {
        for (int i = 0; i < size; i++)
        {
            _pool.Enqueue(CreateObject());
        }
    }

    private T CreateObject()
    {
        T obj = _createFunction();

        if(obj == null)
            throw new InvalidOperationException
                ("Create function returned null.");
        
        obj.OnCreate();
        obj.gameObject.SetActive(false);

        return obj;
    }

    public T Get()
    {
        T obj = _pool.Count > 0 ? 
            _pool.Dequeue() : CreateObject();

        _activeObjects.Add(obj);
        obj.OnGet();
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void Return(T obj)
    {
        if (obj == null || !_activeObjects.Remove(obj))
            return;

        obj.OnReturn();
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_poolRoot);

        _pool.Enqueue(obj);
    }
}
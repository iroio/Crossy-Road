using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] MapGenerator _generator;
    [SerializeField] Transform _player;

    [SerializeField] int _startRowCount = 20;

    [SerializeField] float _frontBuffer = 30f;
    [SerializeField] float _removeDistance = 20f;

    Queue<GameObject> _rows = new Queue<GameObject> ();

    void AddRows()
    {
        List<GameObject> newRows = _generator.SpawnRow();

        foreach (GameObject row in newRows)
        {
            _rows.Enqueue(row);
        }
    }

    void RemoveOldRows()
    {
        while (_rows.Count > 0)
        {
            GameObject row = _rows.Peek();

            float distance = _player.position.z - row.transform.position.z;

            if (distance < _removeDistance) break;

            Destroy(_rows.Dequeue());
        }
    }

    public void CheckPos()
    {
        // 플레이어 앞에 충분한 맵이 없으면 생성
        while (_generator.CurrentZ < _player.position.z + _frontBuffer)
        {
            AddRows();
        }

        RemoveOldRows();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddRows();
    }

    // Update is called once per frame
    void Update()
    {
        CheckPos();
    }
}
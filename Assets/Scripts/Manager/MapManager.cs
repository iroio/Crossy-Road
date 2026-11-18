using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] MapGenerator _generator;
    [SerializeField] Transform _player;

    [SerializeField] float _frontBuffer = 30f;
    [SerializeField] float _removeDistance = 20f;

    Queue<GameObject> _rows = new Queue<GameObject> ();

    // =========================================================
    // 행 생성
    // =========================================================
    bool AddRows()
    {
        int previousZ = _generator.CurrentZ;
        List<GameObject> newRows = _generator.SpawnRow();

        if (_generator.CurrentZ <= previousZ)
        {
            Debug.LogError("Map generation did not advance; disabling MapManager.", this);
            enabled = false;
            return false;
        }

        foreach (GameObject row in newRows)
        {
            _rows.Enqueue(row);
        }

        return true;
    }

    // =========================================================
    // 행 제거
    // =========================================================
    void RemoveOldRows()
    {
        while (_rows.Count > 0)
        {
            GameObject row = _rows.Peek();

            float distance = _player.position.z - row.transform.position.z;

            if (distance < _removeDistance) break;

            PlayerController rider = row.GetComponentInChildren<PlayerController>();
            if (rider != null)
                rider.RemoveFromPlatform();

            if (!_generator.ReleaseRow(row))
            {
                enabled = false;
                return;
            }
            _rows.Dequeue();
        }
    }

    // =========================================================
    // 위치 확인
    // =========================================================
    public void CheckPos()
    {
        // 플레이어 앞에 충분한 맵이 없으면 생성
        while (_generator.CurrentZ < _player.position.z + _frontBuffer)
        {
            if (!AddRows()) return;
        }

        RemoveOldRows();
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        if (_generator == null || _player == null)
        {
            Debug.LogError("MapManager requires a generator and player.", this);
            enabled = false;
            return;
        }

        AddRows();
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        CheckPos();
    }
}
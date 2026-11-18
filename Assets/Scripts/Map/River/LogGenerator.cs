using System.Collections;
using UnityEngine;

public class LogGenerator : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] GameObject _log;

    [SerializeField] int _initialSize = 10;

    [Header("Spawn Delay min-max")]
    [SerializeField] float _min = 0.1f;
    [SerializeField] float _max = 2f;

    Transform _randomPoint;

    GameObjectPool<LogMovement> _logsPools;

    float _spawnDelay;

    bool _isLeft;

    // =========================================================
    // SpawnLog
    // =========================================================
    public void SpawnLog(float angle)
    {
        var log = _logsPools.Get();

        if (log == null) return;

        bool moveRight = _randomPoint == _spawnPoints[0];

        log.transform.position = _randomPoint.position;
        log.transform.rotation = Quaternion.Euler(45f, 0f, moveRight ? 90f : -90f);

        // 이동 방향 전달
        log.SetDirection(moveRight);

        log.gameObject.SetActive(true);
    }

    // =========================================================
    // RemoveLog
    // =========================================================
    public void RemoveLog(LogMovement log)
    {
        PlayerController rider = log.GetComponentInChildren<PlayerController>();
        if (rider != null)
            rider.RemoveFromPlatform();

        log.ResetState();
        _logsPools.Return(log);
    }

    // =========================================================
    // SpawnLog
    // =========================================================
    IEnumerator CoSpawnLog()
    {
        float angle = _randomPoint == _spawnPoints[0] ? 90f : -90f;

        int startCount = 1;

        for (int i = 0; i < startCount; i++)
        {
            SpawnLog(angle);

            yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));
        }

        while (true)
        {
            _spawnDelay = Random.Range(_min, _max);
            yield return new WaitForSeconds(_spawnDelay);

            SpawnLog(angle);
        }
    }

    // =========================================================
    // OnEnable
    // =========================================================
    void OnEnable()
    {
        LogMovement log = _log.GetComponentInChildren<LogMovement>();
        if (_logsPools == null)
        {
            _logsPools = new GameObjectPool<LogMovement>(
                log, _root, _initialSize, log => log.InitLog(this));
        }

        _isLeft = Random.Range(0, 2) == 0;

        _randomPoint = _isLeft ? _spawnPoints[0] : _spawnPoints[1];

        StartCoroutine(CoSpawnLog());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (_logsPools == null) return;

        foreach (LogMovement log in _root.GetComponentsInChildren<LogMovement>(true))
        {
            if (log.gameObject.activeSelf)
                RemoveLog(log);
        }
    }
}


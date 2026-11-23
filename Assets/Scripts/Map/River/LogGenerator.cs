using System.Collections;
using UnityEngine;

public class LogGenerator : MonoBehaviour
{
    ObjectPoolManager _poolManager;

    [SerializeField] Transform _root;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] GameObject _log;

    [SerializeField] int _initialSize = 10;

    [Header("Spawn Delay min-max")]
    [SerializeField] float _min = 0.1f;
    [SerializeField] float _max = 2f;

    Transform _randomPoint;

    const string _poolName = "Log";
    GameObjectPool<LogMovement> _logsPools;

    float _spawnDelay;

    // =========================================================
    // SpawnLog
    // =========================================================
    public void SpawnLog()
    {
        LogMovement log = _logsPools.Get();

        if (log == null) return;

        bool moveRight = _randomPoint == _spawnPoints[0];

        log.transform.position = _randomPoint.position;
        log.transform.rotation = Quaternion.Euler(45f, 0f, moveRight ? 90f : -90f);

        // 이동 방향 전달
        log.SetDirection(moveRight);
    }

    // =========================================================
    // RemoveLog
    // =========================================================
    public void RemoveLog(LogMovement log)
    {
        PlayerController rider = log.GetComponentInChildren<PlayerController>();
        if (rider != null)
            rider.RemoveFromPlatform();

        _logsPools.Return(log);
    }

    // =========================================================
    // SpawnLog
    // =========================================================
    IEnumerator CoSpawnLog()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));

        SpawnLog();

        while (true)
        {
            _spawnDelay = Random.Range(_min, _max);
            yield return new WaitForSeconds(_spawnDelay);

            SpawnLog();
        }
    }

    // =========================================================
    // OnEnable
    // =========================================================
    void OnEnable()
    {
        _poolManager = ObjectPoolManager.Instance;
        if (_logsPools == null)
        {
            var pool = new GameObjectPool<LogMovement>(_root, _initialSize,
                () => Instantiate(_log, _root).GetComponentInChildren<LogMovement>(),
                log => log.InitLog(this));

            _poolManager.RegisterPool(this, _poolName, pool);
        }

        _logsPools = _poolManager.GetPool<LogMovement>(this, _poolName);

        _randomPoint = Random.Range(0, 2) == 0
            ? _spawnPoints[0]
            : _spawnPoints[1];

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

    private void OnDestroy()
    {
        if (_poolManager != null)
            _poolManager.UnregisterPools(this);
    }
}


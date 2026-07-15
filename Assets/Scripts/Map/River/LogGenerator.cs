using System.Collections;
using UnityEngine;

public class LogGenerator : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] GameObject _log;

    [Header("Spawn Delay min-max")]
    [SerializeField] float _min = 0.1f;
    [SerializeField] float _max = 2f;

    Transform _randomPoint;

    GameObjectPool<LogMovement> _logsPools;

    float _spawnDelay;

    bool _isLeft;

    // =========================================================
    // 통나무 프리팹 생성
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
    // 통나무 제거
    // =========================================================
    public void RemoveLog(LogMovement log)
    {
        log.gameObject.SetActive(false);
        log.transform.rotation = Quaternion.identity;
        _logsPools.Set(log);
    }

    // =========================================================
    // 통나무 생성
    // =========================================================
    IEnumerator CoSpawnLog()
    {
        float angle = _randomPoint == _spawnPoints[0] ? 90f : -90f;

        int startCount = Random.Range(0, 1);

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
    // Start
    // =========================================================
    void Start()
    {
        _logsPools = new GameObjectPool<LogMovement>(5, () =>
        {
            var obj = Instantiate(_log, _root);
            obj.SetActive(false);

            var log = obj.GetComponentInChildren<LogMovement>();
            log.InitLog(this);

            return log;
        });

        _isLeft = Random.Range(0, 2) == 0;

        _randomPoint = _isLeft ? _spawnPoints[0] : _spawnPoints[1];

        StartCoroutine(CoSpawnLog());
    }
}

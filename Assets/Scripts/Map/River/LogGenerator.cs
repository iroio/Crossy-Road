using System.Collections;
using UnityEngine;

public class LogGenerator : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] GameObject _log;

    [Header("Spawn Delay min-max")]
    [SerializeField] float _min = 2f;
    [SerializeField] float _max = 5f;

    Transform _randomPoint;

    GameObjectPool<LogMovement> _logsPools;

    float _spawnDelay;

    bool _isLeft;

    // =========================================================
    // 烹唱公 橇府普 积己
    // =========================================================
    public void SpawnLog(float angle)
    {
        var log = _logsPools.Get();

        if (log == null) return;

        log.transform.position = _randomPoint.position;
        log.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        log.gameObject.SetActive(true);
    }

    // =========================================================
    // 烹唱公 力芭
    // =========================================================
    public void RemoveLog(LogMovement log)
    {
        log.gameObject.SetActive(false);
        log.transform.rotation = Quaternion.identity;
        _logsPools.Set(log);
    }

    // =========================================================
    // 烹唱公 积己
    // =========================================================
    IEnumerator CoSpawnLog()
    {
        while (true)
        {
            float angle = _randomPoint == _spawnPoints[0] ? 90f : -90f;

            SpawnLog(angle);

            _spawnDelay = Random.Range(_min, _max);
            yield return new WaitForSeconds(_spawnDelay);
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

            var log = obj.GetComponent<LogMovement>();
            log.InitLog(this);

            return log;
        });

        _isLeft = Random.Range(0, 2) == 0;

        _randomPoint = _isLeft ? _spawnPoints[0] : _spawnPoints[1];

        StartCoroutine(CoSpawnLog());
    }
}

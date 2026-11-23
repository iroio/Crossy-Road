using UnityEngine;

public class TrainGenerator : MonoBehaviour
{
    ObjectPoolManager _poolManager;

    [SerializeField] Transform _train;
    [SerializeField] Transform _root;
    [SerializeField] Transform[] _spawnPoints;

    [SerializeField] int _initialSize = 1;

    Transform _randomPoint;
    Transform _currentTrain;

    bool _isLeft;

    public Transform CurrentTrain => _currentTrain;

    const string _poolName = "Train";
    GameObjectPool<TrainMovement> _trainPool;

    // =========================================================
    // SpawnTrain
    // =========================================================
    public void SpawnTrain()
    {
        if (_trainPool == null) return;
        if (_currentTrain != null) return;

        float angle = _randomPoint == _spawnPoints[0] ? 90f : -90f;
        if (_randomPoint == null) return;

        TrainMovement train = _trainPool.Get();
        
        if (train == null)
        {
            Debug.LogWarning("Failed to get train from pool.", this);
            return;
        }

        train.transform.position = _randomPoint.position;
        train.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        
        _currentTrain = train.transform;
    }

    // =========================================================
    // RemoveTrain
    // =========================================================
    public void RemoveTrain()
    {
        if (_currentTrain == null) return;

        TrainMovement train = _currentTrain.GetComponent<TrainMovement>();
        if (train != null && _trainPool != null)
        {
            _trainPool.Return(train);
        }
        
        _currentTrain = null;
    }

    // =========================================================
    // OnEnable
    // =========================================================
    void OnEnable()
    {
        if (_train == null || _root == null)
        {
            Debug.LogError("Train generator requires a train prefab and pool root.", this);
            enabled = false;
            return;
        }

        _poolManager = ObjectPoolManager.Instance;
        if (_trainPool == null)
        {
            TrainMovement train = _train.GetComponent<TrainMovement>();
            if (train == null)
            {
                Debug.LogError("Train prefab requires a TrainMovement component.", this);
                enabled = false;
                return;
            }

            var pool = new GameObjectPool<TrainMovement>(_root, _initialSize,
                () =>
                {
                    return Instantiate(train, _root);
                },
                train => train.InitTrain(this));

            _poolManager.RegisterPool(this, _poolName, pool);
        }

        _trainPool = _poolManager.GetPool<TrainMovement>(this, _poolName);

        _isLeft = Random.Range(0, 2) == 0;
        _randomPoint = _isLeft ? _spawnPoints[0] : _spawnPoints[1];
    }

    // =========================================================
    // 이 오브젝트가 삭제되면
    // =========================================================
    void OnDestroy()
    {
        RemoveTrain();
        if (_poolManager != null)
            _poolManager.UnregisterPools(this);
    }

    void OnDisable()
    {
        RemoveTrain();
    }
}

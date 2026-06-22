using UnityEngine;
using UnityEngine.InputSystem;

public class TrainGenerator : MonoBehaviour
{
    [SerializeField] Transform _train;
    [SerializeField] Transform[] _spawnPoints;

    Transform _randomPoint;
    Transform _currentTrain;

    bool _isLeft;

    public Transform CurrentTrain => _currentTrain;

    // =========================================================
    // 기차 생성
    // =========================================================
    public void SpawnTrain()
    {
        if (_train == null) return;
        if (_currentTrain != null) return;

        float angle = _randomPoint == _spawnPoints[0] ? 90f : -90f;

        if (_randomPoint == null) return;

        _currentTrain = Instantiate(_train, _randomPoint.position, Quaternion.Euler(0f, angle, 0f));

        _currentTrain.GetComponent<TrainMovement>().InitTrain(this);
    }

    // =========================================================
    // 기차 삭제
    // =========================================================
    public void RemoveTrain()
    {
        if (_currentTrain == null)  return;

        Destroy(_currentTrain.gameObject);
        _currentTrain = null;
    }

    // =========================================================
    // Awake
    // =========================================================
    void Awake()
    {
        _isLeft = Random.value > 0.5f;

        _randomPoint = _isLeft ? _spawnPoints[0] : _spawnPoints[1];
    }
}

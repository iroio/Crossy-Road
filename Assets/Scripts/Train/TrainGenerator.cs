using UnityEngine;
using UnityEngine.InputSystem;

public class TrainGenerator : MonoBehaviour
{
    [SerializeField] Transform _train;
    [SerializeField] Transform[] _spawnPoints;

    Transform _randomPoint;

    bool _isLeft;

    public void SpawnTrain()
    {
        if (_train == null) return;

        float angle = _randomPoint == _spawnPoints[0] ? 90f : -90f;

        var train = Instantiate(_train);
        train.transform.position = _randomPoint.position;
        train.transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    public void RemoveTrain()
    {
        Destroy(_train);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _isLeft = Random.value > 0.5f;

        _randomPoint = _isLeft ? _spawnPoints[0] : _spawnPoints[1];
    }
}

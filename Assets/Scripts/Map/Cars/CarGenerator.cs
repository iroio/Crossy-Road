using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGenerator : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] List<GameObject> _cars = new List<GameObject>();

    [SerializeField] int _initialSize = 10;

    [Header("Spawn Delay min-max")]
    [SerializeField] float _min = 2f;
    [SerializeField] float _max = 4f;

    Transform _randomPoint;

    GameObjectPool<CarMovement> _carsPools;

    float _spawnDelay;

    bool _isLeft;

    // =========================================================
    // 자동차 프리팹 생성
    // =========================================================
    public void SpawnCar(float angle)
    {
        var car = _carsPools.Get();

        if(car == null) return;

        car.transform.position = _randomPoint.position;
        car.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        car.gameObject.SetActive(true);
    }

    // =========================================================
    // 자동차를 월드에 생성
    // =========================================================
    IEnumerator CoSpawnCars()
    {
        float angle = _randomPoint == _spawnPoints[0] ? 90f : -90f;

        int startCount = Random.Range(0, 3);

        for (int i = 0; i < startCount; i++)
        {
            SpawnCar(angle);

            yield return new WaitForSeconds(Random.Range(0.3f, 1f));
        }

        while (true)
        {
            _spawnDelay = Random.Range(_min, _max);
            yield return new WaitForSeconds(_spawnDelay);

            SpawnCar(angle);
        }
    }

    // =========================================================
    // 자동차 제거
    // =========================================================
    public void RemoveCar(CarMovement car)
    {
        car.ResetState();
        _carsPools.Return(car);
    }

    // =========================================================
    // OnEnable
    // =========================================================
    void OnEnable()
    {
        if (_min <= 0f || _max < _min)
        {
            Debug.LogError("Car spawn delays must satisfy 0 < min <= max.", this);
            enabled = false;
            return;
        }

        if (_cars == null || _cars.Count == 0)
        {
            Debug.LogError("Car generator requires at least one car prefab.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < _cars.Count; i++)
        {
            if (_cars[i] == null || _cars[i].GetComponent<CarMovement>() == null)
            {
                Debug.LogError($"Car prefab at index {i} is missing or has no CarMovement component.", this);
                enabled = false;
                return;
            }
        }

        // Pooling
        CarMovement car = _cars[Random.Range(0, _cars.Count)].GetComponent<CarMovement>();
        if (_carsPools == null)
        {
            _carsPools = new GameObjectPool<CarMovement>(
                car, _root, _initialSize, car => car.InitCar(this));
        }

        _isLeft = Random.Range(0, 2) == 0;

        _randomPoint = _isLeft ? _spawnPoints[0] : _spawnPoints[1];

        StartCoroutine(CoSpawnCars());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (_carsPools == null) return;

        foreach (CarMovement car in _root.GetComponentsInChildren<CarMovement>(true))
        {
            if (car.gameObject.activeSelf)
                RemoveCar(car);
        }
    }
}


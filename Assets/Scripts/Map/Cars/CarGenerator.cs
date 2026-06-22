using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class CarGenerator : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] List<GameObject> _cars = new List<GameObject>();

    [Header("Spawn Delay min-max")]
    [SerializeField] float _min = 2f;
    [SerializeField] float _max = 5f;

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
    // 자동차 제거
    // =========================================================
    public void RemoveCar(CarMovement car)
    {
        car.gameObject.SetActive(false);
        car.transform.rotation = Quaternion.identity;
        _carsPools.Set(car);
    }

    // =========================================================
    // 자동차를 월드에 생성
    // =========================================================
    IEnumerator CoSpawnCars()
    {
        while (true)
        {
            float angle =  _randomPoint == _spawnPoints[0] ? 90f : -90f;

            SpawnCar(angle);

            _spawnDelay = Random.Range(_min, _max);
            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        // Pooling
        _carsPools = new GameObjectPool<CarMovement>(12, () =>
        {
            var prefab = _cars[Random.Range(0, _cars.Count)];

            var obj = Instantiate(prefab, _root);
            obj.SetActive(false);

            var car = obj.GetComponent<CarMovement>();
            car.InitCar(this);

            return car;
        });

        _isLeft = Random.value > 0.5f;

        _randomPoint = _isLeft ? _spawnPoints[0] : _spawnPoints[1];

        StartCoroutine(CoSpawnCars());
    }
}

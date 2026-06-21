using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class CarGenerator : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] List<GameObject> _cars = new List<GameObject>();

    Transform _randomPoint;

    GameObjectPool<CarMovement> _carsPools;

    float _spawnDelay;

    bool _isLeft;

    public void SpawnCar(float angle)
    {
        var car = _carsPools.Get();

        if(car == null) return;

        car.transform.position = _randomPoint.position;
        car.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        car.gameObject.SetActive(true);
    }

    public void ReturnCar(CarMovement car)
    {
        car.gameObject.SetActive(false);
        car.transform.rotation = Quaternion.identity;
        _carsPools.Set(car);
    }

    IEnumerator CoSpawnCars()
    {
        while (true)
        {
            float angle =  _randomPoint == _spawnPoints[0] ? 90f : -90f;

            SpawnCar(angle);

            _spawnDelay = Random.Range(2f, 5f);
            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

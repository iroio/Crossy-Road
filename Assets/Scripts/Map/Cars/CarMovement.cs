using UnityEngine;

public class CarMovement : MonoBehaviour
{
    CarGenerator _carGenerator;

    [SerializeField] float _resetRange = 30f;

    [SerializeField] float _speed;

    [Header("Move Speed")]
    [SerializeField] float _min = 4f;
    [SerializeField] float _max = 7f;


    // =========================================================
    // 초기화
    // =========================================================
    public void InitCar(CarGenerator car)
    {
        _carGenerator = car;
    }

    // =========================================================
    //  Start
    // =========================================================
    void Start()
    {
        _speed = Random.Range(_min, _max);
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;

        // x축 길이의 절댓값이 _resetRange 보다 길면
        if (Mathf.Abs(transform.position.x) > _resetRange)
        {
            // 삭제
            _carGenerator.RemoveCar(this);
        }
    }
}

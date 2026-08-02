using UnityEngine;

public class CarMovement : MonoBehaviour
{
    CarGenerator _carGenerator;

    [SerializeField] float _resetRange = 30f;

    [SerializeField] float _speed;

    [Header("Move Speed")]
    [SerializeField] float _min = 4f;
    [SerializeField] float _max = 7f;

    float _targetSpeed;

    // =========================================================
    // Raycast
    // =========================================================
    Vector3 _origin;

    RaycastHit _hit;

    // =========================================================
    // 차량 감지 거리
    // =========================================================
    float _maxDistance = 6f;

    // =========================================================
    // 레이어 확인
    // =========================================================
    int _car;

    // test
    bool _isFrontCarDetected;

    // =========================================================
    // speed 공개
    // =========================================================
    public float Speed => _speed;

    // =========================================================
    // 초기화
    // =========================================================
    public void InitCar(CarGenerator car)
    {
        _carGenerator = car;
    }

    // =========================================================
    // 정면 충돌 확인
    // =========================================================
    public void CheckFront()
    {
        _origin = new Vector3(transform.position.x, 0.5f, transform.position.z);

        // test
        _isFrontCarDetected = false;

        if (Physics.Raycast(_origin, transform.forward, out _hit, _maxDistance, _car))
        {
            _isFrontCarDetected = true;

            CarMovement frontCar = _hit.collider.GetComponent<CarMovement>();

            if (frontCar != null)
            {
                _targetSpeed = Mathf.Lerp(_targetSpeed, frontCar.Speed, Time.deltaTime * 5f);
            }
        }
        else
        {
            _targetSpeed = _speed;
        }
    }
    
// =========================================================
//  Start
// =========================================================
void Start()
    {
        _speed = Random.Range(_min, _max);
        _targetSpeed = _speed * 0.9f;

        _car = LayerMask.GetMask("Car");
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        CheckFront();

        _speed = Mathf.Lerp(_speed, _targetSpeed, Time.deltaTime * 5f);

        transform.position += transform.forward * _speed * Time.deltaTime;

        // x축 길이의 절댓값이 _resetRange 보다 길면
        if (Mathf.Abs(transform.position.x) > _resetRange)
        {
            // 삭제
            _carGenerator.RemoveCar(this);
        }
    }

    // =========================================================
    // 디버그용 Ray 표시
    // =========================================================
    void OnDrawGizmos()
    {
        if (_origin == null)
            return;


        if (_isFrontCarDetected)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;


        Gizmos.DrawRay(
            _origin,
            transform.forward * _maxDistance
        );
    }
}

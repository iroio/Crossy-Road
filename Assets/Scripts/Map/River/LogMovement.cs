using UnityEngine;

public class LogMovement : RidePlatform, IPoolable
{
    LogGenerator _logGenerator;
    
    [SerializeField] float _resetRange = 30f;

    [SerializeField] float _speed;

    bool _moveRight;

    // =========================================================
    // 이동 속도 관련
    // =========================================================
    [Header("Move Speed")]
    [SerializeField] float _startSpeed = 7f;
    [SerializeField] float _min = 2f;
    [SerializeField] float _max = 4f;

    // =========================================================
    // Initialize
    // =========================================================
    public void InitLog(LogGenerator log)
    {
        _logGenerator = log;
    }

    // =========================================================
    // SetDirection
    // =========================================================
    public void SetDirection(bool moveRight)
    {
        _moveRight = moveRight;
    }

    // =========================================================
    // Ipoolable 구현
    // =========================================================
    public void OnCreate() { }
    public void OnGet()
    {
        ResetState();
    }
    public void OnReturn() { }
    public void ResetState()
    {
        _moveRight = false;
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
        // 방향 저장
        Vector3 dir = _moveRight ? Vector3.right : Vector3.left;

        // 통나무 이동
        if (transform.position.x <= -9f || transform.position.x >= 9)
        {
            transform.position += dir * _startSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += dir * _speed * Time.deltaTime;
        }

        // 통나무 삭제
        if (Mathf.Abs(transform.position.x) > _resetRange)
        {
            _logGenerator.RemoveLog(this);
        }
    }
}


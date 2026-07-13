using UnityEngine;

public class LogMovement : MonoBehaviour
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
    // 초기화
    // =========================================================
    public void InitLog(LogGenerator log)
    {
        _logGenerator = log;
    }

    // =========================================================
    // 방향 지정
    // =========================================================
    public void SetDirection(bool moveRight)
    {
        _moveRight = moveRight;
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

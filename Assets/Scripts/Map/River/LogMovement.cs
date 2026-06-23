using UnityEngine;

public class LogMovement : MonoBehaviour
{
    LogGenerator _logGenerator;
    
    [SerializeField] float _resetRange = 30f;

    [SerializeField] float _speed;

    [Header("Move Speed")]
    [SerializeField] float _startSpeed = 7f;
    [SerializeField] float _min = 2f;
    [SerializeField] float _max = 4f;

    // =========================================================
    // √ ±‚»≠
    // =========================================================
    public void InitLog(LogGenerator log)
    {
        _logGenerator = log;
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
        if (transform.position.x <= -9f || transform.position.x >= 9)
        {
            transform.position += transform.forward * _startSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * _speed * Time.deltaTime;
        }

        if (Mathf.Abs(transform.position.x) > _resetRange)
        {
            _logGenerator.RemoveLog(this);
        }
    }
}

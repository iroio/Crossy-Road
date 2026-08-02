using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    GameManager _gameManager;

    // =========================================================
    // 타겟 옵션 설정
    // =========================================================
    [SerializeField] Transform _player;
    [SerializeField] float _speed = 1f;

    // =========================================================
    // 타겟 좌표
    // =========================================================
    Vector3 _targetPos;

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _gameManager = GameManager._GM;
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        if (_gameManager.IsGameOver) return;
        if (_gameManager.IsMain) return;

        transform.position += Vector3.forward * _speed * Time.deltaTime;

        _targetPos = _player.position;
        _targetPos.y = transform.position.y;

        if (_targetPos.z < transform.position.z)
        {
            _targetPos.z = transform.position.z;
        }

        if (transform.position.x != _player.position.x || transform.position.z < _player.position.z)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPos, _speed * Time.deltaTime);
        }
    }
}

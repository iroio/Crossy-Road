using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    GameManager _gameManager;
    LogMovement _logMovement;
    Transform _defaultParent;

    // =========================================================
    // 플레이어 이동 옵션
    // =========================================================
    [SerializeField] float _moveDistance = 1f;
    [SerializeField] float _speed = 1f;
    [SerializeField] float _jumpHeight = 1.2f;

    // 통나무 탑승시 고정 높이
    [SerializeField] float _rideY = 1f;

    // =========================================================
    // 최소 스와이프 거리
    // =========================================================
    [SerializeField] float _minSwipeDistance = 50f;

    // =========================================================
    // 플레이어 크기 옵션
    // =========================================================
    float _minYScale = 0.6f;
    float _maxYScale = 1f;
    float _scaleSpeed = 5f;
    float _floorY = 0f;

    // =========================================================
    // 마우스 위치 체크
    // =========================================================
    Vector2 _startMpos;
    Vector2 _nextMpos;
    Vector2 _mDir;

    // =========================================================
    // Raycast 옵션
    // =========================================================
    Vector3 _origin;
    RaycastHit _hit;

    float _maxDistance = 2.1f;

    // =========================================================
    // 상태 체크
    // =========================================================
    bool _isMoving = false;

    public bool isLog => transform.parent != _defaultParent;

    // =========================================================
    // 레이어 확인
    // =========================================================
    int _layerObstacle;
    int _layerLog;
    int _layerLotus;

    // =========================================================
    // 스와이프 체크
    // =========================================================
    public void SwipeInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            _startMpos = new Vector3(pos.x, pos.y, 0f);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            _nextMpos = new Vector3(pos.x, pos.y, 0f);
        }
    }

    // =========================================================
    // 스와이프 방향 체크
    // =========================================================
    public void SwipeDir()
    {
        if (_isMoving) return;

        Vector3 delta = _nextMpos - _startMpos;

        if (delta.magnitude < _minSwipeDistance)
        {
            if (!IsObstacle(Vector3.forward))
            {
                StartCoroutine(CoCharJump(Vector3.forward));
                StartCoroutine(CoRotation(0));
            }

            return;
        }

        _mDir = delta.normalized;

        if (Mathf.Abs(_mDir.y) > Mathf.Abs(_mDir.x))
        {
            // 앞뒤
            if (_mDir.y > 0)
            {
                if (!IsObstacle(Vector3.forward))
                {
                    StartCoroutine(CoCharJump(Vector3.forward));
                }
            }
            else
            {
                if (!IsObstacle(Vector3.back))
                {
                    StartCoroutine(CoCharJump(Vector3.back));
                    StartCoroutine(CoRotation(180));
                }
            }
        }
        else
        {
            //좌우
            if (_mDir.x > 0)
            {
                if (!IsObstacle(Vector3.right))
                {
                    StartCoroutine(CoCharJump(Vector3.right));
                    StartCoroutine(CoRotation(90));
                }                    
            }
            else
            {
                if (!IsObstacle(Vector3.left))
                {
                    StartCoroutine(CoCharJump(Vector3.left));
                    StartCoroutine(CoRotation(-90));
                }
            }
        }
    }

    // =========================================================
    // 플레이어 회전
    // ========================================================= 
    IEnumerator CoRotation(float target)
    {
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0, target, 0);

        float time = 0f;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }
        transform.rotation = targetRot;
    }

    // =========================================================
    // 장애물 여부 확인
    // ========================================================= 
    public bool IsObstacle(Vector3 dir)
    {
        _origin = transform.position;

        if (Physics.Raycast(_origin, dir, out _hit, _maxDistance, _layerObstacle)) 
        {
            return true;
        }
        return false;
    }

    // =========================================================
    // 점프 처리
    // ========================================================= 
    IEnumerator CoCharJump(Vector3 dir)
    {
        _isMoving = true;
        transform.SetParent(_defaultParent);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dir * _moveDistance;

        float duration = _moveDistance / _speed;
        float time = 0f;

        while (time < duration)
        {
            float progress = time / duration;

            Vector3 nextPos = Vector3.Lerp(startPos, endPos, progress);
            nextPos.y = startPos.y + Mathf.Sin(progress * Mathf.PI) * _jumpHeight;

            transform.position = nextPos;

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        // 착지 후 검사
        CheckLogRide();

        _logMovement = null;

        _isMoving = false;
    }

    // =========================================================
    // 플레이어 크기 변경
    // ========================================================= 
    public void ScaleSize(bool isDown)
    {
        Vector3 scale = transform.localScale;

        if (isDown)
        {
            // 캐릭터 Y축 크기 감소
            scale.y = Mathf.Max(_minYScale, scale.y - _scaleSpeed * Time.deltaTime);
        }
        else
        {
            // 캐릭터 Y축 크기 증가
            scale.y = Mathf.Min(_maxYScale, scale.y + _scaleSpeed * Time.deltaTime);
        }

        transform.localScale = scale;

        // 캐릭터 Y축 높이 감소
        Vector3 pos = transform.position;
        pos.y = _floorY + scale.y * 0.65f;
        transform.position = pos;
    }

    // =========================================================
    // 통나무 확인
    // =========================================================
    public bool CheckLogRide()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out _hit, 2f, _layerLog) || 
            Physics.Raycast(origin, Vector3.down, out _hit, 2f, _layerLotus))
        {
            LogMovement log = _hit.collider.GetComponent<LogMovement>();

            if (log != null)
            {
                // 부모 설정
                transform.SetParent(log.transform);

                Vector3 pos = transform.position;

                pos.y = _hit.collider.bounds.max.y + GetComponent<Collider>().bounds.extents.y;

                transform.position = pos;

                return true;
            }
        }
        return false;
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _gameManager = GameManager._GM;

        _defaultParent = transform.parent;

        _layerObstacle = LayerMask.GetMask("Obstacle");
        _layerLog = LayerMask.GetMask("Log");
        _layerLotus = LayerMask.GetMask("Lotus");
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        if (_gameManager.IsGameOver) return;

        SwipeInput();

        // 플레이어 점프 준비 동작
        if (Mouse.current.leftButton.IsPressed())
        {
            ScaleSize(true);
        }
        else
        {
            ScaleSize(false);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            SwipeDir();
        }
    }
}

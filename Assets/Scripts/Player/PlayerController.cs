using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    GameManager _gameManager;
    MouseManager _mouseManager;
    Transform _defaultParent;

    // =========================================================
    // 플레이어 이동 옵션
    // =========================================================
    [SerializeField] float _moveDistance = 1f;
    [SerializeField] float _speed = 1f;
    [SerializeField] float _jumpHeight = 1.2f;

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
    // 마우스
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
    // 탑승 감지용
    // =========================================================
    Collider _collider;

    // =========================================================
    // 상태 체크
    // =========================================================
    bool _isMoving = false;
    bool _isSwiping = false;

    public bool isLog => transform.parent != _defaultParent;

    // =========================================================
    // 레이어 확인
    // =========================================================
    int _Obstacle;
    int _layerRide;
    int _layerRiver;

    // =========================================================
    // 스와이프 체크
    // =========================================================
    public void SwipeInput()
    {
        Vector2 mousePos = _mouseManager.GetMousePosition();
        if (mousePos == Vector2.zero) return;

        if (_mouseManager.IsClicking())
        {
            Vector2 pos = mousePos;
            _startMpos = new Vector3(pos.x, pos.y, 0f);
            _isSwiping = true;
        }

        if(_mouseManager.IsHolding() && _isSwiping)
        {
            Vector2 pos = mousePos;
            _nextMpos = new Vector3(pos.x, pos.y, 0f);
        }

        if (_mouseManager.IsReleasing() && _isSwiping)
        {
            Vector2 pos = mousePos;
            _nextMpos = new Vector3(pos.x, pos.y, 0f);
            _isSwiping = false;
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
                    StartCoroutine(CoRotation(0));
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

        while (time < duration && !_gameManager.IsGameOver)
        {
            time += Time.deltaTime;

            float t = time / duration;

            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }
        if (!_gameManager.IsGameOver)
            transform.rotation = targetRot;
    }

    // =========================================================
    // 점프 처리
    // ========================================================= 
    IEnumerator CoCharJump(Vector3 dir)
    {
        _isMoving = true;
        transform.SetParent(_defaultParent);

        if(transform.position.x % 2 != 0)
        {
            float nearX = Mathf.Round(transform.position.x / 2f) * 2f;
            transform.position = new Vector3(nearX, transform.position.y, transform.position.z);
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dir * _moveDistance;

        float duration = _moveDistance / _speed;
        float time = 0f;

        while (time < duration && !_gameManager.IsGameOver)
        {
            float progress = time / duration;

            Vector3 nextPos = Vector3.Lerp(startPos, endPos, progress);
            nextPos.y = startPos.y + Mathf.Sin(progress * Mathf.PI) * _jumpHeight;

            transform.position = nextPos;

            time += Time.deltaTime;
            yield return null;
        }

        if (_gameManager.IsGameOver)
        {
            _isMoving = false;
            yield break;
        }

        transform.position = endPos;

        _gameManager.RecordProgress(startPos.z, endPos.z);

        // 착지 후 탑승 검사
        CheckRide();

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
    // 장애물 여부 확인
    // ========================================================= 
    public bool IsObstacle(Vector3 dir)
    {
        _origin = transform.position;

        if (Physics.Raycast(_origin, dir, out _hit, _maxDistance, _Obstacle))
        {
            return true;
        }
        return false;
    }

    // =========================================================
    // 강 플렛폼 탑승 확인
    // =========================================================
    public bool CheckRide()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (!Physics.Raycast(origin, Vector3.down, out _hit, 2f, _layerRide | _layerRiver))
            return true;

        // 레이가 강에 먼저 맞으면 플렛폼이 없음
        if (_hit.collider.gameObject.layer == LayerMask.NameToLayer("River"))
        {
            _gameManager.GameOver();
            return false;
        }

        // 레이가 맞을 플렛폼 식별
        RidePlatform platform = _hit.collider.GetComponentInParent<RidePlatform>();
        if (platform == null)
        {
            _gameManager.GameOver();
            return false;
        }

        transform.SetParent(platform.transform);

        Vector3 pos = transform.position;
        pos.y = _hit.collider.bounds.max.y + _collider.bounds.extents.y;
        transform.position = pos;

        return true;
    }

    public void RemoveFromPlatform()
    {
        transform.SetParent(_defaultParent, true);
        _gameManager.GameOver();
    }

    // =========================================================
    // 자동차 & 기차 & AFK 여부 확인
    // ========================================================= 
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            _gameManager.GameOver();
        }

        if (other.CompareTag("Train"))
        {
            _gameManager.GameOver();
        }

        if (other.CompareTag("CameraTarget"))
        {
            _gameManager.GameOver();
        }
    }

    // =========================================================
    // Awake
    // =========================================================
    void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _gameManager = GameManager.Instance;
        _mouseManager = MouseManager.Instance;

        _defaultParent = transform.parent;

        _Obstacle = LayerMask.GetMask("Obstacle");
        _layerRide = LayerMask.GetMask("LayerRide");
        _layerRiver = LayerMask.GetMask("River");
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        if (_gameManager.IsGameOver) return;

        SwipeInput();

        // 플레이어 점프 준비 동작
        if (_mouseManager.IsClicking())
        {
            ScaleSize(true);
        }
        else
        {
            ScaleSize(false);
        }

        if (_mouseManager.IsReleasing())
        {
            SwipeDir();
        }
    }
}

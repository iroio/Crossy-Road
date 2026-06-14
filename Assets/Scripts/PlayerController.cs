using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
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
    // 마우스 위치 체크
    // =========================================================
    Vector3 _startMpos;
    Vector3 _nextMpos;
    Vector3 _mDir;

    // =========================================================
    // Raycast 옵션
    // =========================================================
    float _maxDistance = 3f;

    // =========================================================
    // 상태 체크
    // =========================================================
    bool _isMoving = false;

    // =========================================================
    // 레이어 확인
    // =========================================================
    int _layerMask;

    // =========================================================
    // 스와이프 체크
    // =========================================================
    public Vector3 SwipeCheck()
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

            return (_nextMpos - _startMpos).normalized;
        }

        return Vector3.zero;
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
                StartCoroutine(CharJump(Vector3.forward));

            return;
        }

        _mDir = delta.normalized;

        if (Mathf.Abs(_mDir.y) > Mathf.Abs(_mDir.x))
        {
            // 앞뒤
            if (_mDir.y > 0)
            {
                if (!IsObstacle(Vector3.forward))
                    StartCoroutine(CharJump(Vector3.forward));

            }
            else
            {
                if (!IsObstacle(Vector3.back))
                    StartCoroutine(CharJump(Vector3.back));
            }
        }
        else
        {
            //좌우
            if (_mDir.x > 0)
            {
                if (!IsObstacle(Vector3.right))
                    StartCoroutine(CharJump(Vector3.right));
            }
            else
            {
                if (!IsObstacle(Vector3.left))
                    StartCoroutine(CharJump(Vector3.left));
            }
        }
    }

    // =========================================================
    // 장애물 여부 확인
    // ========================================================= 
    public bool IsObstacle(Vector3 dir)
    {
        Vector3 origin = transform.position;
        RaycastHit hit;

        if (Physics.Raycast(origin, dir, out hit, _maxDistance, _layerMask)) 
        { 
            return true;
        }
        return false;
    }

    // =========================================================
    // 점프 처리
    // ========================================================= 
    IEnumerator CharJump(Vector3 dir)
    {
        _isMoving = true;

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
        pos.y = _floorY + scale.y * 0.5f;
        transform.position = pos;
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _layerMask = LayerMask.GetMask("Obstacle");
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        SwipeCheck();

        Debug.DrawRay(transform.position, Vector3.forward * _maxDistance, Color.red);
        Debug.DrawRay(transform.position, Vector3.back * _maxDistance, Color.red);
        Debug.DrawRay(transform.position, Vector3.right * _maxDistance, Color.red);
        Debug.DrawRay(transform.position, Vector3.left * _maxDistance, Color.red);

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

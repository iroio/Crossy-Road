using System.Collections;
using UnityEngine;

public class LoadingTitleMovement : MonoBehaviour
{
    // =========================================================
    // 이동 포인트 세팅
    // =========================================================
    [Header("이동 포인트")]
    [SerializeField] RectTransform[] _points;

    // =========================================================
    // 애니메이션 커브
    // =========================================================
    [Header("이동 형태")]
    [SerializeField] AnimationCurve _curve;
    [SerializeField] float _duration = 0.5f;

    RectTransform _rect;

    // =========================================================
    // IN
    // =========================================================
    public IEnumerator CoMoveIn()
    {
        _rect.SetAsLastSibling();

        Vector2 start = _points[0].anchoredPosition;
        Vector2 target = _points[1].anchoredPosition;

        Debug.Log($"Start : {start}");
        Debug.Log($"Target : {target}");
        Debug.Log($"Current : {_rect.anchoredPosition}");

        float time = 0f;

        while (time < _duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / _duration);

            _rect.anchoredPosition = Vector2.Lerp(start, target, _curve.Evaluate(t));

            yield return null;
        }

        _rect.anchoredPosition = target;
    }

    void Awake()
    {
        _rect = GetComponent<RectTransform>();

        _rect.anchoredPosition = _points[0].anchoredPosition;
    }
}

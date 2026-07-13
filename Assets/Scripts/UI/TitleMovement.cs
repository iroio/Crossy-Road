using System.Collections;
using UnityEngine;

public class TitleMovement : MonoBehaviour
{
    // =========================================================
    // 이동 포인트 세팅
    // =========================================================
    [Header("이동 포인트")]
    [SerializeField] RectTransform[] _points;

    // =========================================================
    // 애니메이션 커브
    // =========================================================
    [Header("이동 커브")]
    [SerializeField] AnimationCurve _curve;
    [SerializeField] float[] _durations;

    RectTransform _rect;

    // =========================================================
    // MOVE
    // =========================================================
    IEnumerator Move(int startIndex, int endIndex, float duration)
    {
        Vector2 start = _points[startIndex].anchoredPosition;
        Vector2 target = _points[endIndex].anchoredPosition;

        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);

            _rect.anchoredPosition = Vector2.Lerp(start, target, _curve.Evaluate(t));

            yield return null;
        }

        _rect.anchoredPosition = target;
    }

    // =========================================================
    // IN
    // =========================================================
    public IEnumerator CoMoveIn()
    {
        _rect.SetAsLastSibling();

        yield return Move(0, 1, _durations[0]);
    }

    // =========================================================
    // OUT
    // =========================================================
    public IEnumerator CoMoveOut()
    {
        int length = _points.Length;

        for (int i = 0; i < length - 1; i++)
        {
            yield return Move(i, i + 1, _durations[i]);
        }
    }

    // =========================================================
    // Awake
    // =========================================================
    void Awake()
    {
        _rect = GetComponent<RectTransform>();

        _rect.anchoredPosition = _points[0].anchoredPosition;
    }
}

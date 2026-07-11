using System.Collections;
using Unity.VisualScripting;
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
    [Header("이동 형태")]
    [SerializeField] AnimationCurve _curve;
    [SerializeField] float[] _durations;

    RectTransform _rect;

    // =========================================================
    // OUT
    // =========================================================
    public IEnumerator CoMoveOut()
    {
        int length = _points.Length;

        for (int i = 1 ; i < length; i++)
        {
            Vector2 start = _rect.anchoredPosition;
            Vector2 target = _points[i].anchoredPosition;

            float duration = _durations[i - 1];
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;

                float t = Mathf.Clamp01(time / duration);

                _rect.anchoredPosition = Vector2.Lerp(start, target, _curve.Evaluate(t));

                yield return null;
            }

            _rect.anchoredPosition = target;
        }
    }

    void Awake()
    {
        _rect = GetComponent<RectTransform>();

        _rect.anchoredPosition = _points[0].anchoredPosition;
    }
}

using System.Collections;
using UnityEngine;

public class FadeManager : Singleton<FadeManager>
{
    [SerializeField] CanvasGroup _canvasGroup;
    public bool IsConfigured => _canvasGroup != null; // ¼öÁ¤

    [SerializeField] float _fadeTime = 0.5f;

    // =========================================================
    // Fade In
    // =========================================================
    public IEnumerator CoFadeIn()
    {
        float time = 0f;

        while (time < _fadeTime)
        {
            time += Time.deltaTime;

            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / _fadeTime);

            yield return null;
        }

        _canvasGroup.alpha = 0f;
    }

    // =========================================================
    // Fade Out
    // =========================================================
    public IEnumerator CoFadeOut()
    {
        float time = 0f;

        while(time < _fadeTime)
        {
            time += Time.deltaTime;

            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / _fadeTime);

            yield return null;
        }

        _canvasGroup.alpha = 1f;
    }
}

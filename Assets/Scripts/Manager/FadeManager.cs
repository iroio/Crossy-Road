using System.Collections;
using UnityEngine;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] CanvasGroup _canvasGroup;

    [SerializeField] float _fadeTime = 0.5f;

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

    // =========================================================
    // Fade In
    // =========================================================
    public IEnumerator CoFadeIn()
    {
        Debug.Log(_canvasGroup);

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
    // Awake ¹× ½Ì±ÛÅæ
    // =========================================================
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

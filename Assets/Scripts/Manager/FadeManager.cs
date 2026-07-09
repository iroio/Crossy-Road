using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] CanvasGroup _canvasGroup;

    [SerializeField] float _fadeTime = 0.5f;

    public void TestFade()
    {
        StartCoroutine(CoTestFade());
    }

    IEnumerator CoTestFade()
    {
        yield return CoFadeOut();

        yield return new WaitForSeconds(1f);

        yield return CoFadeIn();
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

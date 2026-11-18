using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : Singleton<ScenesManager>
{
    bool _isLoading;
    bool _loadSucceeded;

    // =========================================================
    // 씬 로드
    // =========================================================
    public void LoadScene(string sceneName)
    {
        if (_isLoading) return;
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName)) return;
        if (!Application.CanStreamedLevelBeLoaded("Load")) return;

        FadeManager fadeManager = FindAnyObjectByType<FadeManager>(); // 수정
        if (fadeManager == null || !fadeManager.IsConfigured) // 수정
        {
            Debug.LogError("Scene transition requires a configured FadeManager.", this); // 수정
            return; // 수정
        }

        _isLoading = true;
        StartCoroutine(CoLoadScene(sceneName, fadeManager)); // 수정
    }

    // =========================================================
    // 비동기 씬 로드
    // =========================================================
    IEnumerator Load(string sceneName)
    {
        _loadSucceeded = false;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            yield break;
        }

        AsyncOperation op = null;
        string error = null;
        try
        {
            op = SceneManager.LoadSceneAsync(sceneName);
        }
        catch (System.ArgumentException exception)
        {
            error = exception.Message;
        }
        catch (System.InvalidOperationException exception)
        {
            error = exception.Message;
        }

        if (op == null)
        {
            yield break;
        }

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log("Waiting to Finish Loading");

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        _loadSucceeded = true;
        Debug.Log("Done");

        yield return null;
    }

    // =========================================================
    // 페이드 인 아웃 흐름
    // =========================================================
    IEnumerator CoLoadScene(string sceneName, FadeManager fadeManager) // 수정
    {
        string previousScene = SceneManager.GetActiveScene().path;
        try
        {
            // 페이드 아웃
            Debug.Log("FadeOut Start");
            yield return StartCoroutine(fadeManager.CoFadeOut()); // 수정

            // Load 씬 이동
            yield return StartCoroutine(Load("Load"));
            if (!_loadSucceeded)
            {
                yield return StartCoroutine(fadeManager.CoFadeIn()); // 수정
                yield break;
            }

            // 타이틀 IN
            TitleMovement loadingTitle = FindAnyObjectByType<TitleMovement>();
            if (loadingTitle != null)
                yield return loadingTitle.CoMoveIn();
            else
                Debug.LogError("The Load scene has no TitleMovement; continuing without its animation.", this);

            // 0.5초 대기
            yield return new WaitForSeconds(0.5f);

            // 씬 로드
            yield return StartCoroutine(Load(sceneName));
            if (!_loadSucceeded && !string.IsNullOrEmpty(previousScene))
            {
                Debug.LogError($"Returning to the previous scene after failing to load '{sceneName}'.", this);
                yield return StartCoroutine(Load(previousScene));
            }

            // 초기화
            if (_loadSucceeded && SceneManager.GetActiveScene().name == "Game")
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResetGame();
                }
                else
                    Debug.LogError("The Game scene has no GameManager.", this);
            }

            // 페이드 인
            yield return StartCoroutine(fadeManager.CoFadeIn()); // 수정
        }
        finally
        {
            _isLoading = false;
        }
    }
}

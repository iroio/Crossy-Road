using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance;
    bool _isLoading;
    bool _loadSucceeded;

    // =========================================================
    // æ¿ ∑ŒµÂ
    // =========================================================
    public void LoadScene(string sceneName)
    {
        if (_isLoading)
        {
            Debug.LogWarning("A scene transition is already in progress.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Cannot load scene '{sceneName}': it is not available in the build settings.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded("Load") || FadeManager.Instance == null)
        {
            Debug.LogError("Scene transition requires the Load scene and FadeManager.", this);
            return;
        }

        _isLoading = true;
        StartCoroutine(CoLoadScene(sceneName));
    }

    // =========================================================
    // ∫Òµø±‚ æ¿ ∑ŒµÂ
    // =========================================================
    IEnumerator Load(string sceneName)
    {
        _loadSucceeded = false;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Cannot load scene '{sceneName}': it is not available in the build settings.", this);
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
            Debug.LogError($"Failed to load scene '{sceneName}': {error ?? "no load operation was returned"}.", this);
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
    // ∆‰¿ÃµÂ ¿Œ æ∆øÙ »Â∏ß
    // =========================================================
    IEnumerator CoLoadScene(string sceneName)
    {
        string previousScene = SceneManager.GetActiveScene().path;
        try
        {
            // ∆‰¿ÃµÂ æ∆øÙ
            Debug.Log("FadeOut Start");
            yield return StartCoroutine(FadeManager.Instance.CoFadeOut());

            // Load æ¿ ¿Ãµø
            yield return StartCoroutine(Load("Load"));
            if (!_loadSucceeded)
            {
                yield return StartCoroutine(FadeManager.Instance.CoFadeIn());
                yield break;
            }

            // ≈∏¿Ã∆≤ IN
            TitleMovement loadingTitle = FindAnyObjectByType<TitleMovement>();
            if (loadingTitle != null)
                yield return loadingTitle.CoMoveIn();
            else
                Debug.LogError("The Load scene has no TitleMovement; continuing without its animation.", this);

            // 0.5√  ¥Î±‚
            yield return new WaitForSeconds(0.5f);

            // æ¿ ∑ŒµÂ
            yield return StartCoroutine(Load(sceneName));
            if (!_loadSucceeded && !string.IsNullOrEmpty(previousScene))
            {
                Debug.LogError($"Returning to the previous scene after failing to load '{sceneName}'.", this);
                yield return StartCoroutine(Load(previousScene));
            }

            // √ ±‚»≠
            if (_loadSucceeded && SceneManager.GetActiveScene().name == "Game")
            {
                if (GameManager._GM != null)
                    GameManager._GM.ResetGame();
                else
                    Debug.LogError("The Game scene has no GameManager.", this);
            }

            // ∆‰¿ÃµÂ ¿Œ
            yield return StartCoroutine(FadeManager.Instance.CoFadeIn());
        }
        finally
        {
            _isLoading = false;
        }
    }

    // =========================================================
    // Awake π◊ ΩÃ±€≈Ê ¿˚øÎ
    // =========================================================
    void Awake()
    {
        if (Instance == null)
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

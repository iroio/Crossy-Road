using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    TitleMovement _loadingTitle;

    public static ScenesManager Instance;

    // =========================================================
    // 씬 로드
    // =========================================================
    public void LoadScene(string sceneName)
    {
        StartCoroutine(CoLoadScene(sceneName));
    }

    // =========================================================
    // 비동기 씬 로드
    // =========================================================
    IEnumerator Load(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            Debug.Log("Loading");
            yield return null;
        }

        Debug.Log("Waiting to Finish Loading");

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        Debug.Log("Done");

        yield return null;
    }

    // =========================================================
    // 페이드 인 아웃 흐름
    // =========================================================
    IEnumerator CoLoadScene(string sceneName)
    {
        // 페이드 아웃
        Debug.Log("FadeOut Start");
        yield return StartCoroutine(FadeManager.Instance.CoFadeOut());

        // Load 씬 이동
        yield return StartCoroutine(Load("Load"));

        // 타이틀 IN
        TitleMovement loadingTitle = FindFirstObjectByType<TitleMovement>();
        yield return loadingTitle.CoMoveIn();

        // 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 씬 로드
        yield return StartCoroutine(Load(sceneName));

        // 초기화
        if (sceneName == "Game")
        {
            GameManager._GM.ResetGame();
        }

        // 페이드 인
        yield return StartCoroutine(FadeManager.Instance.CoFadeIn());
    }

    // =========================================================
    // Awake 및 싱글톤 적용
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

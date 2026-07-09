using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance;

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

        if (sceneName == "Game")
        {
            GameManager._GM.StartGame();
        }
    }

    IEnumerator CoLoadScene(string sceneName)
    {
        // È­¸é Fade out
        yield return FadeManager.Instance.CoFadeOut();

        // ¾À ·Îµå
        yield return StartCoroutine(Load(sceneName));
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(CoLoadScene(sceneName));
    }

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

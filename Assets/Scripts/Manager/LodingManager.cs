using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LodingManager : MonoBehaviour
{
    // =========================================================
    // 게잉이 Load 씬으로 넘어오면 실행
    // =========================================================

    IEnumerator Start()
    {
        yield return StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        Debug.Log("1");

        AsyncOperation op = SceneManager.LoadSceneAsync("Game");

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            Debug.Log($"Loading : {op.progress}");
            yield return null;
        }

        Debug.Log("2");

        yield return new WaitForSeconds(0.3f);

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        Debug.Log("3");

        GameManager._GM.ResetGame();

        Debug.Log("4");

        yield return StartCoroutine(FadeManager.Instance.CoFadeIn());

        Debug.Log("5");
    }
}

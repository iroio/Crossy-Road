using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LodingManager_Del : MonoBehaviour
{
    // =========================================================
    // 게잉이 Load 씬으로 넘어오면 실행
    // =========================================================

    IEnumerator Start()
    {
        yield return StartCoroutine(LoadGame());
    }

    // =========================================================
    // LoadGame
    // =========================================================
    IEnumerator LoadGame()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Game");

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            Debug.Log($"Loading : {op.progress}");
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        GameManager._GM.ResetGame();

        yield return StartCoroutine(FadeManager.Instance.CoFadeIn());
    }
}

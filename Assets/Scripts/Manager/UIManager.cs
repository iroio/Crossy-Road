using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _scoreTmp;
    [SerializeField] TextMeshProUGUI _resultTmp;
    [SerializeField] Canvas _canvas;

    public static UIManager _UM;

    bool _isLoading = false;
    bool _isGameOver = false;

    public void OnClickPlay()
    {
        if (_isLoading) return;

        _isLoading = true;

        GameManager._GM.ResetGame();

        ScenesManager.Instance.LoadScene("Game");
    }

    public void onClickRetry()
    {
        Debug.Log("Retry Click");

        if (_isLoading) return;

        _isLoading = true;

        ScenesManager.Instance.LoadScene("Load");
    }

    public void ChangeScore(int score)
    {
        _scoreTmp.text = score.ToString();
    }

    public void GameOverResult(int highScore)
    {
        _canvas.gameObject.SetActive(true);
        _resultTmp.text = highScore.ToString();
        _isGameOver = true;
    }

    public void LoadTitle()
    {
        GameManager._GM.ResetGame();
        ScenesManager.Instance.LoadScene("Title");
    }

    public void Awake()
    {
        _UM = this;

        _isLoading = false;
        _isGameOver = false;

        if (_canvas != null)
            _canvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_isGameOver)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name == "Title")
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnClickPlay();
            }
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            FadeManager.Instance.TestFade();
        }
    }
}

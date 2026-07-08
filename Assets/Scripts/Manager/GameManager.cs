using UnityEngine;

public enum GameState
{
    Main,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager _GM;

    int _score = 0;
    int _highScore;

    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsGameOver => CurrentState == GameState.GameOver;
    public bool IsMain => CurrentState == GameState.Main;

    public GameState CurrentState { get; private set; }

    public void AddScore(int score)
    {
        _score += score;
    }

    public void ReStartGame()
    {
        ChangeState(GameState.Playing);
        _score = 0;
    }

    public void GameOver()
    {
        Debug.Log("Game Over");

        ChangeState(GameState.GameOver);
        // 최고 기록

    }

    public void BackToMain()
    {
        ChangeState(GameState.Main);
        _score = 0;
    }

    public void ChangeState(GameState state)
    {
        CurrentState = state;

        switch (state)
        {
            case GameState.Main:

                break;

            case GameState.Playing:

                break;

            case GameState.GameOver:

                break;
        }
    }

    private void Awake()
    {
        if (_GM == null)
        {
            _GM = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //_score = PlayerPrefs.GetInt("HighScore", 0);
    }
}

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _GM;

    int _score = 0;

    bool _isGameOver;
    bool _isPlaying;

    public void AddScore(int score)
    {
        _score += score;
    }

    public void StartGame()
    {
        _isPlaying = true;
        _isGameOver = false;
        _score = 0;
    }

    public void GameOver()
    {
        Debug.Log("Game Over");

        _isGameOver = true;
    }

    public void ResetGame()
    {
        _isGameOver = false;
        _isPlaying = false;
        _score = 0;
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

        _score = PlayerPrefs.GetInt("HighScore", 0);
    }
}

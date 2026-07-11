using UnityEngine;

// =========================================================
// 게임 상태
// =========================================================
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

    // =========================================================
    // 게임 상태 프로퍼티
    // =========================================================
    public bool IsMain => CurrentState == GameState.Main;
    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsGameOver => CurrentState == GameState.GameOver;

    public GameState CurrentState { get; private set; }

    // =========================================================
    // 점수 증가
    // =========================================================
    public void AddScore(int score)
    {
        _score += score;

        if(UIManager._UM != null)
            UIManager._UM.ChangeScore(_score);
    }

    // =========================================================
    // 게임 시작
    // =========================================================
    public void StartGame()
    {
        _score = 0;

        ChangeState(GameState.Playing);
    }

    // =========================================================
    // 게임 오버
    // =========================================================
    public void GameOver()
    {
        Debug.Log("Game Over");

        ChangeState(GameState.GameOver);

        //최고기록
        if (_score > _highScore)
        {
            _highScore = _score;
            PlayerPrefs.SetInt("HighScore", _highScore);
            PlayerPrefs.Save();
        }

        //Result 텍스트 출력
        if (UIManager._UM != null)
            UIManager._UM.GameOverResult(_highScore);
    }

    // =========================================================
    // 게임 초기화
    // =========================================================
    public void ResetGame()
    {
        _score = 0;
        ChangeState(GameState.Main);
    }

    // =========================================================
    // 게임 상태 변경 
    // =========================================================
    public void ChangeState(GameState state)
    {
        CurrentState = state;

        if (UIManager._UM == null) return;

        switch (state)
        {
            case GameState.Main:
                UIManager._UM.ShowMain();
                break;

            case GameState.Playing:
                UIManager._UM.ShowPlaying();
                break;

            case GameState.GameOver:
                UIManager._UM.ShowGameOver();
                break;
        }
    }

    // =========================================================
    // Awake 및 싱글톤 적용 
    // =========================================================
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

        _highScore = PlayerPrefs.GetInt("HighScore", 0);

        CurrentState = GameState.Main;
    }
}
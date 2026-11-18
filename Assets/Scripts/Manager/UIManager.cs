using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    // =========================================================
    // UI 패널 연결
    // =========================================================
    [SerializeField] GameObject _mainPanel;
    [SerializeField] GameObject _gamePanel;
    [SerializeField] GameObject _resultPanel;
    [SerializeField] GameObject _ButtonPanel;

    // =========================================================
    // 움직일 대상
    // =========================================================
    [SerializeField] TitleMovement _titleMovement;

    // =========================================================
    // 점수
    // =========================================================
    [SerializeField] TextMeshProUGUI _scoreTmp;
    [SerializeField] TextMeshProUGUI _resultTmp;

    public static UIManager _UM;

    bool _mouseMissing;

    // =========================================================
    // 버튼
    // =========================================================
    public void OnClickRetry()
    {
        if (ScenesManager.Instance == null) return;

        ScenesManager.Instance.LoadScene("Game");
    }

    // =========================================================
    // 점수 변경
    // =========================================================
    public void ChangeScore(int score)
    {
        _scoreTmp.text = score.ToString();
    }

    // =========================================================
    // 게임 오버 결과 출력
    // =========================================================
    public void GameOverResult(int highScore)
    {
        _resultTmp.text = highScore.ToString();

        ShowGameOver();
    }

    // =========================================================
    // Title 블러오기
    // =========================================================
    public void LoadTitle()
    {
        OnClickRetry();
    }

    // =========================================================
    // Main 일때 UI 상태
    // =========================================================
    public void ShowMain()
    {
        _mainPanel.SetActive(true);
        _gamePanel.SetActive(false);
        _resultPanel.SetActive(false);
        _ButtonPanel.SetActive(false);
    }

    // =========================================================
    // 게임 중 UI 상태
    // =========================================================
    public void ShowPlaying()
    {
        _mainPanel.SetActive(true);
        _gamePanel.SetActive(true);
        _resultPanel.SetActive(false);
        _ButtonPanel.SetActive(false);
    }

    // =========================================================
    // 게임오버일 때 UI 상태
    // =========================================================
    public void ShowGameOver()
    {
        _mainPanel.SetActive(false);
        _gamePanel.SetActive(true);
        _resultPanel.SetActive(true);
        _ButtonPanel.SetActive(true);
    }

    // =========================================================
    // Awake
    // =========================================================
    public void Awake()
    {
        if(_UM == null)
        {
            _UM = this;
        }
        else if(_UM != this)
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        GameManager.Instance.ChangeState(GameState.Main);
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        if (!GameManager.Instance.IsMain) return;

        MouseManager mouse = MouseManager.Instance;
        if (mouse == null)
        {
            if (!_mouseMissing)
                Debug.LogWarning("Starting the game requires a mouse.", this);
            _mouseMissing = true;
            return;
        }

        _mouseMissing = false;
        if (mouse.IsClicking())
        {
            GameManager.Instance.StartGame();

            StartCoroutine(_titleMovement.CoMoveOut());
        }
    }
}

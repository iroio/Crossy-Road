using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _scoreTmp;
    [SerializeField] TextMeshProUGUI _resultTmp;
    [SerializeField] Canvas _canvas;

    public static UIManager _UM;

    public void ShowMain()
    {
        Debug.Log("Main");
    }

    public void ShowGame()
    {
        Debug.Log("Playing");
    }

    public void ShowGameOver()
    {
        Debug.Log("GameOver");
    }

    public void Awake()
    {
        if(_UM==null)
        {
            _UM = this;

            DontDestroyOnLoad(gameObject);

            if (_canvas != null)
                _canvas.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            switch (GameManager._GM.CurrentState)
            {
                case GameState.Main:
                    GameManager._GM.ReStartGame();
                    break;

                case GameState.GameOver:
                    GameManager._GM.BackToMain();
                    break;
            }
        }
    }
}

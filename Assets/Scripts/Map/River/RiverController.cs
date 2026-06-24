using UnityEngine;

public class RiverController : MonoBehaviour
{
    GameManager _gameManager;


    // =========================================================
    // 게임오버 여부 확인
    // ========================================================= 
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _gameManager.GameOver();
        }
    }

    void Start()
    {
        _gameManager = GameManager._GM;
    }
}

using System.Collections;
using UnityEngine;

public class RiverController : MonoBehaviour
{
    [SerializeField] GameObject _splashPrefab;

    GameManager _gameManager;

    IEnumerator Sink(Transform target)
    {
        Vector3 start = target.position;
        Vector3 end = start + Vector3.down * 5f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            target.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    // =========================================================
    // 게임오버 여부 확인
    // ========================================================= 
    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        // 중복 실행 방지
        if (player == null) return;

        // 통나무?
        if (player.CheckLogRide()) return;

        _gameManager.GameOver();

        // 게임오버 판정 이후 진행할 로직

        // 이펙트 생성
        Instantiate(_splashPrefab, player.transform.position, _splashPrefab.transform.rotation);

        // 위치 내리기
        StartCoroutine(Sink(player.transform));
    }

    void Start()
    {
        _gameManager = GameManager._GM;
    }
}

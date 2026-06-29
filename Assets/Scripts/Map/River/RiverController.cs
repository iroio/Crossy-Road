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
        if (other.CompareTag("Player"))
        {
            _gameManager.GameOver();

            // 게임오버 판정 이후 진행할 로직

            // 이펙트 생성
            Instantiate(_splashPrefab, other.transform.position, _splashPrefab.transform.rotation);

            // 위치 내리기
            StartCoroutine(Sink(other.transform));


        }
    }

    void Start()
    {
        _gameManager = GameManager._GM;
    }
}

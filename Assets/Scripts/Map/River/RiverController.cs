using System.Collections;
using UnityEngine;

public class RiverController : MonoBehaviour
{
    [SerializeField] GameObject _splashPrefab;

    [SerializeField] int _initialSize = 5;

    GameManager _gameManager;

    const string _poolName = "Splash";
    GameObjectPool<SpalshPool> _splashPool;

    // =========================================================
    // Sink the character
    // =========================================================
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
    // OnTriggerEnter
    // ========================================================= 
    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null) return;

        if (player.CheckRide()) return;

        _gameManager.GameOver();

        PlaySplash(player.transform.position);

        StartCoroutine(Sink(player.transform));
    }

    // =========================================================
    // PlaySplash
    // =========================================================
    void PlaySplash(Vector3 position)
    {
        if (_splashPool == null) return;

        SpalshPool splash = _splashPool.Get();
        if (splash == null) return;

        splash.transform.position = position;
        splash.OnGet();

        StartCoroutine(ReturnSplashAfterDelay(splash, 2f));
    }

    // =========================================================
    // 일정 시간 후 Splash 반환
    // =========================================================
    IEnumerator ReturnSplashAfterDelay(SpalshPool splash, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (_splashPool != null && splash != null)
        {
            _splashPool.Return(splash);
        }
    }

    private void OnEnable()
    {
        if (_splashPrefab == null)
        {
            Debug.LogError("Splash Prefab이 할당되지 않았습니다.", this);
            enabled = false;
            return;
        }

        SpalshPool splash = _splashPrefab.GetComponentInChildren<SpalshPool>();

        if (splash == null)
        {
            Debug.LogError($"SpalshPool 컴포넌트를 찾을 수 없습니다: " +
                $"{_splashPrefab.name}", _splashPrefab);

            enabled = false;
            return;
        }

        _splashPool = new GameObjectPool<SpalshPool>(transform, _initialSize, 
            ()=> Instantiate(splash, transform),
            splash => splash.OnGet());

        ObjectPoolManager.Instance.RegisterPool(_poolName, _splashPool);
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _gameManager = GameManager.Instance;
    }
}

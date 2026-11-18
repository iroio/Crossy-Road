using System.Collections;
using UnityEngine;

public class RiverController : MonoBehaviour
{
    [SerializeField] GameObject _splashPrefab;

    [SerializeField] int _initialSize = 5;

    GameManager _gameManager;

    GameObjectPool<ParticleSystem> _splashPool;

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

        ParticleSystem splash = _splashPool.Get();
        if (splash == null) return;

        splash.transform.position = position;
        splash.Play();
        
        StartCoroutine(ReturnSplashAfterDelay(splash, 2f));
    }

    // =========================================================
    // 일정 시간 후 Splash 반환
    // =========================================================
    IEnumerator ReturnSplashAfterDelay(ParticleSystem splash, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (_splashPool != null && splash != null)
        {
            _splashPool.Return(splash);
        }
    }

    private void OnEnable()
    {
        ParticleSystem splash = _splashPrefab.GetComponentInChildren<ParticleSystem>();
        _splashPool = new GameObjectPool<ParticleSystem>(
            splash, transform, _initialSize, splash => splash.Play());
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _gameManager = GameManager.Instance;
    }
}

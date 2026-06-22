using System.Collections;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class LotusGenerator : MonoBehaviour
{
    [SerializeField] GameObject _lotus;

    Transform _randomPoint;

    int _randomSpawnPointX;

    GameObjectPool<LotusMovement> _lotusPools;

    // =========================================================
    // 楷采蕾 橇府普 积己
    // =========================================================
    public void SpawnLotus(float angle)
    {
        var lotus = _lotusPools.Get();

        if (lotus == null) return;

        lotus.transform.position = _randomPoint.position;
        lotus.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        lotus.gameObject.SetActive(true);
    }

    // =========================================================
    // 楷采蕾 力芭
    // =========================================================
    public void RemoveLog(LotusMovement lotus)
    {
        lotus.gameObject.SetActive(false);
        lotus.transform.rotation = Quaternion.identity;
        _lotusPools.Set(lotus);
    }

    // =========================================================
    // 烹唱公甫 岿靛俊 积己
    // =========================================================
    IEnumerator CoSpawnLog()
    {
        while (true)
        {
            SpawnLotus();

            yield return null;
        }
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _randomSpawnPointX = Random.Range(-8, 9);


        _lotusPools = new GameObjectPool<LotusMovement>(3, () =>
        {
            var obj = Instantiate(_lotus);
            obj.SetActive(false);

            var lotus = obj.GetComponent<LotusMovement>();
            lotus.InitLotus(this);

            return lotus;
        });
    }
}

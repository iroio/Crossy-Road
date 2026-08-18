using System.Collections.Generic;
using UnityEngine;

public class NatureGenerator : MonoBehaviour
{
    [SerializeField] GameObject[] _natures;
    [SerializeField] Transform _root;

    [Header("Natures Spawn Count min-max")]
    [SerializeField] int _naturesMin = 1;
    [SerializeField] int _naturesMax = 4;

    int _randomCount;

    GameObjectPool<Transform>[] _naturesPools;
    List<int> _naturePos = new List<int>();

    // =========================================================
    // Pool 선택
    // =========================================================
    private GameObjectPool<Transform> GetNaturePool()
    {
        int random = Random.Range(0, _naturesPools.Length);

        return _naturesPools[random];
    }

    // =========================================================
    // 자연물 프리팹 생성
    // =========================================================
    public Transform GetNature()
    {
        GameObjectPool<Transform> pool = GetNaturePool();

        Transform obj = pool.Get();

        if (obj == null) return null;

        NaturesReturn natures = obj.GetComponent<NaturesReturn>();

        if (natures != null)
        {
            natures.SetPool(pool);
        }

        obj.gameObject.SetActive(true);

        return obj;
    }

    // =========================================================
    // 자연물 프리팹 생성 (위치지정 O)
    // =========================================================
    public void SpawnNature(Transform row, int x)
    {
        Transform obj = GetNature();

        if (obj == null) return;

        obj.position = new Vector3(x, 0f, row.position.z);

        obj.SetParent(_root);
    }

    // =========================================================
    // 자연물 생성 내부
    // =========================================================
    public void SpawnNatureIn(Transform row)
    {
        _randomCount = Random.Range(_naturesMin, _naturesMax + 1);

        // spawn 가능한 위치 초기화
        _naturePos.Clear();

        for (int x = -8; x <= 8; x += 2)
        {
            _naturePos.Add(x);
        }

        // 중복 없이 랜덤한 위치 선택
        for (int i = 0; i < _randomCount; i++)
        {
            int index = Random.Range(0, _naturePos.Count);
            int randomX = _naturePos[index];

            _naturePos.RemoveAt(index);

            SpawnNature(row, randomX);
        }
    }

    // =========================================================
    // 자연물 생성 외부
    // =========================================================
    public void SpawnNatureOut(Transform row)
    {
        for (int x = -20; x < -10; x += 2)
        {
            SpawnNature(row, x);
        }

        for (int x = 12; x <= 20; x += 2)
        {
            SpawnNature(row, x);
        }
    }

    // =========================================================
    // 행 하나에 자연물 생성
    // =========================================================
    public void SpawnNatureGroup(Transform row)
    {
        // 내부 영역 생성
        SpawnNatureIn(row);

        // 외부 영역 생성
        SpawnNatureOut(row);
    }

    // =========================================================
    // 자연물 제거
    // =========================================================
    public void Removenature(NaturesReturn natures)
    {
        natures.ReturnNature();
    }

    // =========================================================
    // Awake (Pooling)
    // =========================================================
    void Awake()
    {
        _naturesPools = new GameObjectPool<Transform>[_natures.Length];

        int length = _natures.Length;
        for (int i = 0; i < length; i++)
        {
            GameObject prefab = _natures[i];

            _naturesPools[i] = new GameObjectPool<Transform>(5, () =>
            {
                Transform obj = Instantiate(prefab).transform;
                obj.gameObject.SetActive(false);
                return obj;
            });
        }
    }
}

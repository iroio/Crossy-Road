using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] NatureGenerator _natureGenerator;
    [SerializeField] Transform _root;

    [SerializeField] int _initialSize = 10;

    /// <summary>
    /// 밝은 색 타일과 어두운 색 타일이 번갈아 나와야 함
    /// Scriptable Object로 번갈아 생성하는 것 보다
    /// 다른 타일들을 Scriptable Object로 관리하고
    /// 잔디 타일만 따로 관리하기 위해 프리팹을 따로 등록
    /// </summary>
    [SerializeField]  GameObject _lightGrassPrefab;
    [SerializeField]  GameObject _darkGrassPrefab;

    [SerializeField]  List<RowData> _rowDatas;

    const string _poolName = "Row";
    readonly Dictionary<GameObject, GameObjectPool<RowObject>> _rowPools 
        = new Dictionary<GameObject, GameObjectPool<RowObject>>();
    
    //fix: Track registered pools to avoid duplicate registration
    readonly Dictionary<GameObject, bool> _registeredPools = new Dictionary<GameObject, bool>();

    readonly Dictionary<GameObject, GameObject> _rowPrefabs 
        = new Dictionary<GameObject, GameObject>();

    Transform _inactiveRowsRoot;

    int _currentZ = 2;

    public int CurrentZ => _currentZ;

    // =========================================================
    // 각 행 가중치 계산
    // =========================================================
    private RowData GetRandomRow()
    {
        if (_rowDatas == null || _rowDatas.Count == 0)
        {
            Debug.LogError("Map generation requires at least one row configuration.", this);
            return null;
        }

        // 가중치의 총 합
        int totalWeight = 0; 

        // 비교용 변수
        int currentWeight = 0;

        // List 전체 순회
        foreach (var row in _rowDatas)
        {
            if (row == null)
            {
                Debug.LogError("Map generation has a missing row configuration.", this);
                return null;
            }

            if (row.weight < 0 || row.minRepeat < 1 || row.maxRepeat < row.minRepeat ||
                row.maxRepeat == int.MaxValue || row.weight > int.MaxValue - totalWeight ||
                (row.rowType != RowType.Grass && row.prefab == null) ||
                (row.rowType == RowType.Grass && (_lightGrassPrefab == null || _darkGrassPrefab == null || _natureGenerator == null)))
            {
                Debug.LogError($"Invalid map row configuration: {row.name}.", row);
                return null;
            }

            // 각 행의 가충치 합
            totalWeight += row.weight;
        }

        // 0~가중치 총합 사이의 값중 랜덤으로 숫자 하나 추출
        if (totalWeight == 0)
        {
            Debug.LogError("Map row weights must have a positive total.", this);
            return null;
        }

        int randomValue = Random.Range(0, totalWeight);

        // 다시 전체 List 순회
        foreach (var row in _rowDatas)
        {
            currentWeight += row.weight;

            /// <summary>
            /// 랜덤으로 선택된 수와 현재 가중치 비교
            /// ex) 랜덤 수 : 45
            ///      현재 가중치 : 50 
            ///      45 < 50  --> 참 으로 행 선택
            ///      랜덤 수 : 73
            ///      현재 가중치 : 50
            ///      73 < 50  --> 거짓으로 다음으로 넘어감
            ///      현재 가중치 : 50 + 30 = 80
            ///      73 < 80  --> 참 으로 다음 행 선택
            /// </summary>
            if (randomValue < currentWeight)
            {
                // 선택된 행 리턴
                return row;
            }
        }

        return null;
    }

    // =========================================================
    // 행 생성
    // =========================================================
    public List<GameObject> SpawnRow()
    {
        // 반환할 리스트 변수
        List<GameObject> spawnedRows = new List<GameObject>();

        // 가중치를 이용해 행 한줄 선택
        RowData row = GetRandomRow();

        if (row == null) return spawnedRows;

        // minRepeat ~ maxRepeat + 1 에서 하나의 숫자 선택
        // maxRepeat 바로 이전 숫까까지 중에 선택하므로 maxRepeat + 1
        int repeat = Random.Range(row.minRepeat, row.maxRepeat + 1);

        for (int i = 0; i < repeat; i++)
        {
            // 실제 생성할 프리팹을 저장할 변수
            GameObject prefabToSpawn;

            bool isGrass = row.rowType == RowType.Grass;

            // 선택된 행이 Grass 이면
            if (isGrass)
            {
                // 짝수 행에 어두운 타일 배치
                // 홀수 행에 밝은 타일 배치
                int rowIndex = _currentZ / 2;

                prefabToSpawn = (rowIndex % 2 == 0) ? _darkGrassPrefab : _lightGrassPrefab;
            }
            else
            {
                // 아니면 별도의 과정 없이 행 선택
                prefabToSpawn = row.prefab;
            }

            // 행 생성
            GameObject newRow = GetRow(prefabToSpawn, _currentZ);

            // Grass면 자연물 생성
            if (isGrass)
            {
                _natureGenerator.SpawnNatureGroup(newRow.transform);
            }

            spawnedRows.Add(newRow);

            // 생성 행 위치 2칸 증가
            _currentZ += 2;
        }

        return spawnedRows;
    }

    GameObject GetRow(GameObject prefab, int z)
    {
        if (_inactiveRowsRoot == null)
        {
            GameObject root = new GameObject("Inactive Row Pool");
            root.SetActive(false);
            root.transform.SetParent(_root, false);
            _inactiveRowsRoot = root.transform;
        }

        if (!_rowPools.TryGetValue(prefab, out GameObjectPool<RowObject> pool))
        {
            pool = new GameObjectPool<RowObject>(_inactiveRowsRoot, _initialSize,
                ()=> Instantiate(prefab, _inactiveRowsRoot).GetComponent<RowObject>());

            _rowPools.Add(prefab, pool);

            // fix with AI
            // fix: Register row pool to Ob/ectPoolManager on first pool creation
            if (!_registeredPools.ContainsKey(prefab))
            {
                string poolKey = $"Row{prefab.name}";
                ObjectPoolManager.Instance.RegisterPool(poolKey, pool);
                _registeredPools[prefab] = true;
            }
        }

        RowObject row = pool.Get();

        _rowPrefabs[row.gameObject] = prefab;

        row.transform.SetParent(_root, false);
        row.transform.SetPositionAndRotation(new Vector3(0, 0, z), Quaternion.identity);

        return row.gameObject;
    }

    public bool ReleaseRow(GameObject row)
    {
        if (row == null || !_rowPrefabs.TryGetValue(row, out GameObject prefab)) return false;
        if (!row.activeSelf) return false;

        foreach (NaturesReturn nature in row.GetComponentsInChildren<NaturesReturn>())
        {
            _natureGenerator.Removenature(nature);
        }

        _rowPrefabs.Remove(row);

        row.transform.SetParent(_inactiveRowsRoot, false);
        _rowPools[prefab].Return(row.GetComponent<RowObject>());

        return true;
    }
}

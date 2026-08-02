using System.Collections.Generic;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineRoll;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] NatureGenerator _natureGenerator;
    [SerializeField] Transform _root;

    /// <summary>
    /// 밝은 색 타일과 어두운 색 타일이 번갈아 나와야 함
    /// Scriptable Object로 번갈아 생성하는 것 보다
    /// 다른 타일들을 Scriptable Object로 관리하고
    /// 잔디 타일만 따로 관리하기 위해 프리팹을 따로 등록
    /// </summary>
    [SerializeField]  GameObject _lightGrassPrefab;
    [SerializeField]  GameObject _darkGrassPrefab;

    [SerializeField]  List<RowData> _rowDatas;

    int _currentZ = 2;

    bool _isGrass = false;
    public int CurrentZ => _currentZ;

    // =========================================================
    // 각 행 가중치 계산
    // =========================================================
    public RowData GetRandomRow()
    {
        // 가중치의 총 합
        int totalWeight = 0; 

        // 비교용 변수
        int currentWeight = 0;

        // List 전체 순회
        foreach (var row in _rowDatas)
        {
            // 각 행의 가충치 합
            totalWeight += row.weight;
        }

        // 0~가중치 총합 사이의 값중 랜덤으로 숫자 하나 추출
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

            // 선택된 행이 Grass 이면
            if (row.rowType == RowType.Grass)
            {
                // 짝수 행에 어두운 타일 배치
                // 홀수 행에 밝은 타일 배치
                int rowIndex = _currentZ / 2;

                prefabToSpawn = (rowIndex % 2 == 0) ? _darkGrassPrefab : _lightGrassPrefab;

                _isGrass = true;
            }
            else
            {
                // 아니면 별도의 과정 없이 행 선택
                prefabToSpawn = row.prefab;
                _isGrass = false;
            }

            // 행 생성
            GameObject newRow =  Instantiate(prefabToSpawn, new Vector3(0, 0, _currentZ), Quaternion.identity, _root);

            // Grass면 자연물 생성
            if (_isGrass)
            {
                _natureGenerator.SpawnNatureGroup(newRow.transform);
            }

            spawnedRows.Add(newRow);

            // 생성 행 위치 2칸 증가
            _currentZ += 2;
        }

        return spawnedRows;
    }
}

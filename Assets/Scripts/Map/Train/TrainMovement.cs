using Unity.VisualScripting;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    TrainGenerator _trainGenerator;

    [SerializeField] float _speed;
    [SerializeField] float _resetRange = 50f;

    Vector3 _startPos;

    // =========================================================
    // 초기화
    // =========================================================
    public void InitTrain(TrainGenerator generator)
    {
        _trainGenerator = generator;
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _startPos = transform.position;
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    { 
        if (_trainGenerator == null)  return;
        
        transform.position += transform.forward * _speed * Time.deltaTime;

        // 시작점에서 현재 오브젝트 까지의 거리가 200을 넘는다면
        if (Vector3.Distance(_startPos, transform.position) > 200f)
        {
            // 삭제
            _trainGenerator.RemoveTrain();
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CrossingGateMovement : MonoBehaviour
{ 
    [SerializeField] TrainGenerator _trainGenerator;

    Coroutine _corail;

    Animator _animator;

    Vector3 _newPos;

    float _idleTime;

    int _randomX;

    bool _isEnd;

    // =========================================================
    // 애니메이션 종료
    // =========================================================
    public void AnimEnd()
    {
        _isEnd = true;
    }

    // =========================================================
    // 차단기 애니메이션 종료 후 기차 생성 로직
    // =========================================================
    IEnumerator CoRail()
    {

        while (true)
        {
            _isEnd = false;

            _idleTime = Random.Range(5f, 10f);

            yield return new WaitForSeconds(_idleTime);

            _animator.SetBool("isActive", true);

            // _isEnd가 True일 때 까지 기다리기
            yield return new WaitUntil(() => _isEnd);

            _animator.SetBool("isActive", false);

            _trainGenerator.SpawnTrain();

            // _trainGenerator.CurrentTrain이 True일 때 까지 기다리기
            yield return new WaitUntil(() =>  _trainGenerator.CurrentTrain == null);
        }
    }

    // =========================================================
    // 생성 위치 설정
    // =========================================================
    public void SetPosition()
    {
        _randomX = Random.Range(-9, 6);

        _newPos = new Vector3(_randomX, 0f, transform.position.z);

        transform.position = _newPos;
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    { 
        _animator = GetComponent<Animator>();

        SetPosition();

        if(_corail == null)
            _corail = StartCoroutine(CoRail());
    }

    // =========================================================
    // 이 오브젝트가 삭제되면
    // =========================================================
    void OnDestroy()
    {
        StopAllCoroutines();
    }
}

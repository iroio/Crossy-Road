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

    public void AnimEnd()
    {
        _isEnd = true;
    }

    IEnumerator CoRail()
    {

        while (true)
        {
            _isEnd = false;

            _idleTime = Random.Range(5f, 10f);

            yield return new WaitForSeconds(_idleTime);

            _animator.SetBool("isActive", true);

            yield return new WaitUntil(() => _isEnd);

            _animator.SetBool("isActive", false);

            _trainGenerator.SpawnTrain();

            yield return new WaitUntil(() =>  _trainGenerator.CurrentTrain == null);
        }
    }

    public void SetPosition()
    {
        _randomX = Random.Range(-9, 6);

        _newPos = new Vector3(_randomX, 0f, transform.position.z);

        transform.position = _newPos;
    }

    void Start()
    { 
        _animator = GetComponent<Animator>();

        SetPosition();

        if(_corail == null)
            _corail = StartCoroutine(CoRail());
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}

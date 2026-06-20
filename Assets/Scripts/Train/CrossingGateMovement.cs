using System.Collections;
using UnityEngine;

public class CrossingGateMovement : MonoBehaviour
{ 
    TrainGenerator _trainGenerator;
    Animator _animator;

    float _idleTime;
    float _spawnDelay;

    IEnumerator CoAnimationStart()
    {
        while(true)
        {
            _animator.SetBool("isActive", true);
            yield return new WaitForSeconds(_idleTime);

            _animator.SetBool("isActive", false);
            yield return new WaitForSeconds(_idleTime);
        }
    }

    IEnumerator CoEnimCrossingGate()
    {
        while (true)
        {
            _trainGenerator.SpawnTrain();

            _spawnDelay = Random.Range(0f, 2f);
            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();

        StartCoroutine(CoAnimationStart());
    }
}

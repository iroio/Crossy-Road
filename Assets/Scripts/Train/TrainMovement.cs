using Unity.VisualScripting;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    TrainGenerator _trainGenerator;

    [SerializeField] float _speed;
    [SerializeField] float _resetRange = 50f;

    Vector3 _startPos;

    public void InitTrain(TrainGenerator generator)
    {
        _trainGenerator = generator;
    }

    void Start()
    {
        _startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    { 
        if (_trainGenerator == null)  return;
            
        transform.position += transform.forward * _speed * Time.deltaTime;

        if (Vector3.Distance(_startPos, transform.position) > 200f)
        {
            _trainGenerator.RemoveTrain();
        }
    }
}

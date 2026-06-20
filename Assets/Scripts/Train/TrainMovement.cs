using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    TrainGenerator _trainGenerator;

    [SerializeField] float _speed;
    [SerializeField] float _resetRange = 50f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) > _resetRange)
        {
            _trainGenerator.RemoveTrain();
        }
    }
}

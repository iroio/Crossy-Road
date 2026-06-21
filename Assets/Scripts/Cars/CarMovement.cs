using UnityEngine;

public class CarMovement : MonoBehaviour
{
    CarGenerator _carGenerator;

    [SerializeField] float _resetRange = 30f;

    [SerializeField] float _speed;

    public void InitCar(CarGenerator car)
    {
        _carGenerator = car;
    }

    void Start()
    {
        _speed = Random.Range(4f, 7f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;

        if(Mathf.Abs(transform.position.x) > _resetRange)
        {
            _carGenerator.ReturnCar(this);
        }
    }
}

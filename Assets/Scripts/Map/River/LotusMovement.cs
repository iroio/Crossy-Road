using UnityEngine;

public class LotusMovement : MonoBehaviour
{
    LotusGenerator _lotusGenerator;

    [SerializeField] float _angle = 30f;
    [SerializeField] float _speed = 10f;

    public void InitLotus(LotusGenerator lotus)
    {
        _lotusGenerator = lotus;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}

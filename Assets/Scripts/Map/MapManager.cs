using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] MapGenerator _generator;
    [SerializeField] Transform _player;

    private Queue<GameObject> _rows = new Queue<GameObject> ();

    Vector3 _playerPos;

    public void CheckPos()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

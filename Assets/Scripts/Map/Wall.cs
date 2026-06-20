using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] Transform _player;

    Vector3 _localPos;

    void Start()
    {
        _localPos = transform.position;
    }

    void Update()
    {
        _localPos.z = _player.position.z;
        transform.position = _localPos;
    }
}

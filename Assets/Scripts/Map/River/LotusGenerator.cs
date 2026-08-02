using System.Collections.Generic;
using UnityEngine;

public class LotusGenerator : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] GameObject _lotus;

    [Header("Lotus Spawn Count min-max")]
    [SerializeField] int _lotusMin = 2;
    [SerializeField] int _lotusMax = 4;

    int _randomCount;

    GameObjectPool<LotusMovement> _lotusPools;
    List<int> _lotusPos = new List<int>();

    // =========================================================
    // ¿¬²ÉÀÙ ÇÁ¸®ÆÕ »ý¼º
    // =========================================================
    public void SpawnLotus(float angle, int x)
    {
        var lotus = _lotusPools.Get();

        if (lotus == null) return;

        lotus.transform.position = new Vector3(x, 0.15f, transform.position.z);
        lotus.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        lotus.gameObject.SetActive(true);
    }

    // =========================================================
    // ¿¬²ÉÀÙ »ý¼º
    // =========================================================
    void SpawnLotusGroup()
    {
        float angle = Random.Range(0, 360f);
        _randomCount = Random.Range(_lotusMin, _lotusMax + 1);

        // spawn °¡´ÉÇÑ À§Ä¡ ÃÊ±âÈ­
        _lotusPos.Clear();

        for (int x = -8; x <= 8; x += 2) 
        {
            _lotusPos.Add(x);
        }

        // Áßº¹ ¾øÀÌ ·£´ýÇÑ À§Ä¡ ¼±ÅÃ
        for (int i = 0; i < _randomCount; i++)
        {
            int index = Random.Range(0, _lotusPos.Count);
            int randomX = _lotusPos[index];

            _lotusPos.RemoveAt(index);

            SpawnLotus(angle, randomX);
        }
    }

    // =========================================================
    // ¿¬²ÉÀÙ Á¦°Å
    // =========================================================
    public void RemoveLotus(LotusMovement lotus)
    {
        lotus.gameObject.SetActive(false);
        lotus.transform.rotation = Quaternion.identity;
        _lotusPools.Set(lotus);
    }

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        _lotusPools = new GameObjectPool<LotusMovement>(3, () =>
        {
            var obj = Instantiate(_lotus, _root);
            obj.SetActive(false);

            var lotus = obj.GetComponent<LotusMovement>();
            lotus.InitLotus(this);

            return lotus;
        });

        SpawnLotusGroup();
    }
}

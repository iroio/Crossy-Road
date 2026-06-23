using UnityEngine;

public class LotusMovement : MonoBehaviour
{
    LotusGenerator _lotusGenerator;

    [SerializeField] float _angle = 30f;
    [SerializeField] float _speed = 10f;

    // =========================================================
    // 초기화
    // =========================================================
    public void InitLotus(LotusGenerator lotus)
    {
        _lotusGenerator = lotus;
    }

    // =========================================================
    //  Start
    // =========================================================
    void Start()
    {
        
    }

    // =========================================================
    // 이 오브젝트가 삭제되면
    // =========================================================
    void OnDestroy()
    {
        StopAllCoroutines();

        _lotusGenerator.RemoveLotus(this);
    }
}

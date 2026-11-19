using UnityEngine;

public class LotusMovement : RidePlatform, IPoolable
{
    LotusGenerator _lotusGenerator;

    // =========================================================
    // Ipoolable 구현
    // =========================================================
    public void OnCreate() { }
    public void OnGet()
    {
        ResetState();
    }
    public void OnReturn() { }
    public void ResetState() 
    {
        // 추후에 추가 예정
    }

    public void InitLotus(LotusGenerator lotusGenerator)
    {
        _lotusGenerator = lotusGenerator;
    }
}

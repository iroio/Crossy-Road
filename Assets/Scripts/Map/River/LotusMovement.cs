using UnityEngine;

public class LotusMovement : RidePlatform
{
    LotusGenerator _lotusGenerator;

    public void InitLotus(LotusGenerator lotusGenerator)
    {
        _lotusGenerator = lotusGenerator;
    }
}

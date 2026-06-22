using UnityEngine;

public enum RowType
{
    Grass,
    Road,
    Rail,
    River1,
    River2
}

[CreateAssetMenu(menuName = "Map/Row Data")]
public class RowData : ScriptableObject
{
    public RowType rowType;

    public GameObject prefab;

    // ÃâÇö È®·ü
    [Range(0, 100)]
    public int weight;

    public int minRepeat;
    public int maxRepeat;
}
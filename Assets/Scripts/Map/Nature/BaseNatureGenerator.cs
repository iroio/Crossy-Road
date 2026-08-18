using UnityEngine;

public class BaseNatureGenerator : MonoBehaviour
{
    [SerializeField] NatureGenerator _natureGenerator;

    // =========================================================
    // 자연물 생성
    // =========================================================
    private void SpawnBase(int min, int max, bool center)
    {
        for (int x = min; x <= max; x += 2)
        {
            if (center && x > -12 && x < 12)
                continue;

            Transform obj = _natureGenerator.GetNature();

            if (obj == null) continue;

            obj.SetParent(transform);

            obj.position = new Vector3(x, 0f, transform.position.z);
        }
    }

    // =========================================================
    // 자연물 생성 외부
    // =========================================================
    public void SpawnBaseOut()
    {
        SpawnBase(-20, 20, false);
    }

    // =========================================================
    // 자연물 생성 내부
    // =========================================================
    public void SpawnBaseIn()
    {
        SpawnBase(-20, 20, true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.position.z >= -12 && transform.position.z < 1)
        {
            if(transform.position.z >= -6 && transform.position.z < 1)
                SpawnBaseIn();
            else
                SpawnBaseOut();
        }
    }
}

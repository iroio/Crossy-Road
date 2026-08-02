using UnityEngine;

public class BaseNatureGenerator : MonoBehaviour
{
    [SerializeField] NatureGenerator _natureGenerator;

    // =========================================================
    // 자연물 생성
    // =========================================================
    public void SpawnBaseOut()
    {
        for (int x = -20; x <= 20; x += 2)
        {
            Transform obj = _natureGenerator.GetNature();

            obj.SetParent(transform);

            obj.position = new Vector3(x, 0f, transform.position.z);
        }
    }

    // =========================================================
    // 자연물 생성 내부 비우기
    // =========================================================
    public void SpawnBaseIn()
    {
        for (int x = -20; x <= 20; x += 2)
        {
            if (x > -12 && x < 12)
                continue;

            Transform obj = _natureGenerator.GetNature();

            obj.SetParent(transform);

            obj.position = new Vector3(x, 0f, transform.position.z);
        }
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

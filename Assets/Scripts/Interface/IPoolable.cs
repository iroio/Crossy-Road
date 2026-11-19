public interface IPoolable
{
    // 처음 생성될 때 호출되는 메서드
    void OnCreate();

    // 오브젝트가 풀에서 가져올 때 호출되는 메서드
    void OnGet();

    // 오브젝트가 풀로 반환될 때 호출되는 메서드
    void OnReturn();
}
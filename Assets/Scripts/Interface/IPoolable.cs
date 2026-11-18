public interface IPoolable
{
    void OnCreate();
    void OnGet();
    void OnReturn();
}
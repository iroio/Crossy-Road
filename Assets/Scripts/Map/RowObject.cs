using UnityEngine;

public class RowObject : MonoBehaviour, IPoolable
{
    public void OnCreate() { }

    public void OnGet() { }

    public void OnReturn() { }
}
using UnityEngine;

public class SpalshPool : MonoBehaviour, IPoolable
{
    private ParticleSystem _particle;

    public void OnCreate()
    {
        _particle = GetComponent<ParticleSystem>();
    }

    public void OnGet()
    {
        if (_particle == null) return;

        _particle.Play();
    }

    public void OnReturn()
    {
        if (_particle == null) return;

        _particle.Stop();
    }
}
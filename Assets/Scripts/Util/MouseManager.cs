using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : Singleton<MouseManager>
{
    public bool IsClicking()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
        return false;
    }

    public bool IsReleasing()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasReleasedThisFrame;
        }
        return false;
    }

    public bool IsHolding()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.isPressed;
        }
        return false;
    }

    public Vector2 GetMousePosition()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        return Vector2.zero;
    }
}

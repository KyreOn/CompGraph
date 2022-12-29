using OpenTK;
using OpenTK.Input;

namespace CompGraph.Tools;

public static class MouseManager
{
    private static MouseState lastMouseState;
    private static MouseState thisMouseState;

    public static void Update()
    {
        lastMouseState = thisMouseState;
        thisMouseState = Mouse.GetState();
        Mouse.GetCursorState();
    }
    public static Vector2 DeltaPosition => new(thisMouseState.X - lastMouseState.X, thisMouseState.Y - lastMouseState.Y);
}
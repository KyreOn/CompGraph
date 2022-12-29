using OpenTK.Input;

namespace CompGraph.Tools;

static class KeyboardManager
{
    private static KeyboardState _lastKeyboardState;
    private static KeyboardState _curKeyboardState;
    public static void Update()
    {
        _lastKeyboardState = _curKeyboardState;
        _curKeyboardState = Keyboard.GetState();
    }
        
    public static bool IsKeyTouched(Key key) => _curKeyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyUp(key);
    public static bool IsKeyDown(Key key) => _curKeyboardState.IsKeyDown(key);
}
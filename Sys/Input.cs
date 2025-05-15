using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace mmd_sd.Helpers;

public static class Input
{
    private static KeyboardState _currentKeyboardState;
    private static MouseState _currentMouseState;
    private static Vector2 _lastMousePosition;
    private static bool _firstMouse = true;

    public static float MouseWheelDelta { get; private set; }
    public static Vector2 MouseDelta { get; private set; }
    public static Vector2 MousePosition => _currentMouseState.Position;

    public static float Horizontal =>
        (GetKey(Keys.D) ? 1f : 0f) - (GetKey(Keys.A) ? 1f : 0f);

    public static float Vertical =>
        (GetKey(Keys.W) ? 1f : 0f) - (GetKey(Keys.S) ? 1f : 0f);

    public static void UpdateState(KeyboardState keyboard, MouseState mouse)
    {
        _currentKeyboardState = keyboard;
        _currentMouseState = mouse;

        var currentPosition = mouse.Position;

        MouseWheelDelta = mouse.ScrollDelta.Y;

        Console.WriteLine(MouseWheelDelta);

        if (_firstMouse)
        {
            _lastMousePosition = currentPosition;
            MouseDelta = Vector2.Zero;
            _firstMouse = false;
        }
        else
        {
            MouseDelta = currentPosition - _lastMousePosition;
            _lastMousePosition = currentPosition;
        }
    }
    
    public static bool GetMouseButtonDown(MouseButton button)
    {
        return _currentMouseState.IsButtonPressed(button);
    }

    public static bool GetMouseButton(MouseButton button)
    {
        return _currentMouseState.IsButtonDown(button);
    }
    
    public static bool GetKeyDown(Keys key)
    {
        return _currentKeyboardState.IsKeyPressed(key);
    }

    public static bool GetKeyUp(Keys key)
    {
        return !_currentKeyboardState.IsKeyReleased(key);
    }

    public static bool GetKey(Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key);
    }
}
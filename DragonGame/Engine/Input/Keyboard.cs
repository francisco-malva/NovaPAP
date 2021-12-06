using SDL2;

namespace DuckDuckJump.Engine.Input;

internal static class Keyboard
{
    private static readonly unsafe bool* _keyState;

    static unsafe Keyboard()
    {
        _keyState = (bool*)SDL.SDL_GetKeyboardState(out _);
    }

    /// <summary>
    ///     Is the key down?
    /// </summary>
    public static unsafe bool KeyDown(SDL.SDL_Scancode scancode)
    {
        return _keyState[(int)scancode];
    }
}

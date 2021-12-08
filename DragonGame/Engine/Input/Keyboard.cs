using System;
using SDL2;

namespace DuckDuckJump.Engine.Input;

internal static class Keyboard
{
    private const int ScancodeCount = (int)SDL.SDL_Scancode.SDL_NUM_SCANCODES;
    private static readonly unsafe bool* _keyState;

    private static readonly bool[] _previousState = new bool[ScancodeCount];
    private static readonly bool[] _currentState = new bool[ScancodeCount];

    static unsafe Keyboard()
    {
        _keyState = (bool*)SDL.SDL_GetKeyboardState(out _);
    }

    public static void Update()
    {
        Array.Copy(_currentState, _previousState, ScancodeCount);
        unsafe
        {
            for (var i = 0; i < ScancodeCount; i++) _currentState[i] = _keyState[i];
        }
    }

    public static bool KeyDown(SDL.SDL_Scancode scancode)
    {
        return !_previousState[(int)scancode] && _currentState[(int)scancode];
    }

    public static bool KeyUp(SDL.SDL_Scancode scancode)
    {
        return _previousState[(int)scancode] && !_currentState[(int)scancode];
    }

    /// <summary>
    ///     Is the key down?
    /// </summary>
    public static bool KeyHeld(SDL.SDL_Scancode scancode)
    {
        return _currentState[(int)scancode];
    }
}
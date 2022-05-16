#region

using System;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Input;

/// <summary>
///     The class responsible for presenting an abstraction to keyboard inputs.
/// </summary>
internal static class Keyboard
{
    private const int ScancodeCount = (int)SDL.SDL_Scancode.SDL_NUM_SCANCODES;
    private static readonly unsafe bool* KeyState;

    private static readonly bool[] PreviousState = new bool[ScancodeCount];
    private static readonly bool[] CurrentState = new bool[ScancodeCount];

    static unsafe Keyboard()
    {
        KeyState = (bool*)SDL.SDL_GetKeyboardState(out _);
    }

    public static bool AnyHeld(out SDL.SDL_Scancode key)
    {
        for (var i = 0; i < CurrentState.Length; i++)
        {
            if (!KeyHeld((SDL.SDL_Scancode)i)) continue;
            key = (SDL.SDL_Scancode)i;
            return true;
        }

        key = 0;
        return false;
    }

    public static bool AnyUp(out SDL.SDL_Scancode key)
    {
        for (var i = 0; i < CurrentState.Length; i++)
        {
            if (!KeyUp((SDL.SDL_Scancode)i)) continue;
            key = (SDL.SDL_Scancode)i;
            return true;
        }

        key = 0;
        return false;
    }

    public static bool AnyDown(out SDL.SDL_Scancode key)
    {
        for (var i = 0; i < CurrentState.Length; i++)
        {
            if (!KeyDown((SDL.SDL_Scancode)i)) continue;
            key = (SDL.SDL_Scancode)i;
            return true;
        }

        key = 0;
        return false;
    }

    /// <summary>
    ///     Updates the internal state of this class.
    /// </summary>
    public static void Update()
    {
        //Make the last cached keyboard state be the previous keyboard state.
        Array.Copy(CurrentState, PreviousState, ScancodeCount);
        unsafe
        {
            //Update the internal state array to correspond to the current keyboard state.
            for (var i = 0; i < ScancodeCount; i++) CurrentState[i] = KeyState[i];
        }
    }

    /// <summary>
    ///     Was this key just pressed this frame?
    /// </summary>
    /// <param name="scancode">The scancode corresponding to the key to be tested.</param>
    /// <returns>A boolean that answers the query.</returns>
    public static bool KeyDown(SDL.SDL_Scancode scancode)
    {
        return !PreviousState[(int)scancode] && CurrentState[(int)scancode];
    }

    /// <summary>
    ///     Was this key just let go this frame?
    /// </summary>
    /// <param name="scancode">The scancode corresponding to the key to be tested.</param>
    /// <returns>A boolean that answers the query.</returns>
    public static bool KeyUp(SDL.SDL_Scancode scancode)
    {
        return PreviousState[(int)scancode] && !CurrentState[(int)scancode];
    }

    /// <summary>
    ///     Is this key being pressed?
    /// </summary>
    /// <param name="scancode">The scancode corresponding to the key to be tested.</param>
    /// <returns>A boolean that answers this query.</returns>
    public static bool KeyHeld(SDL.SDL_Scancode scancode)
    {
        return CurrentState[(int)scancode];
    }
}
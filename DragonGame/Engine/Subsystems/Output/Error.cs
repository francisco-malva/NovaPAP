// ReSharper disable RedundantUsingDirective

#region

using System;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.States;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Output;

public static class Error
{
    public static void RaiseMessage(string message)
    {
        SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", message, Graphics.Window);
    }

    public static void RaiseException(Exception exception)
    {
#if DEBUG
        throw exception;
#else
            GameFlow.Set(new MainMenuState());
            Error.RaiseMessage(exception.Message);
#endif
    }
}
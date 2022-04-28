﻿#region

using System;
using DuckDuckJump.Engine.Subsystems.Graphical;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Output;

public static class Error
{
    public static void Panic(string message)
    {
        SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", message, Graphics.Window);
        Environment.Exit(-1);
    }
}
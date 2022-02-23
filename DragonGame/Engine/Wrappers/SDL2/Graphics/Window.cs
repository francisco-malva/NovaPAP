using System;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics;

internal class Window : IDisposable
{
    public Window(string title, int x, int y, int w, int h, SDL.SDL_WindowFlags flags)
    {
        Handle = SDL.SDL_CreateWindow(title, x, y, w, h, flags);
    }

    public IntPtr Handle { get; private set; }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources()
    {
        SDL.SDL_DestroyWindow(Handle);
        Handle = IntPtr.Zero;
    }

    ~Window()
    {
        ReleaseUnmanagedResources();
    }
}
using System;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.TTF;

internal class Font : IDisposable
{
    private readonly IntPtr _handle;


    public Font(string file, int ptSize)
    {
        _handle = SDL_ttf.TTF_OpenFont(file, ptSize);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public Surface RenderTextSolid(string text, Color color)
    {
        return new Surface(SDL_ttf.TTF_RenderText_Solid(_handle, text, color.Value));
    }

    private void ReleaseUnmanagedResources()
    {
        SDL_ttf.TTF_CloseFont(_handle);
    }

    ~Font()
    {
        ReleaseUnmanagedResources();
    }
}
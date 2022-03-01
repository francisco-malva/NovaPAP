using System;
using System.Drawing;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.TTF;

internal class Font : IDisposable
{
    private readonly IntPtr _handle;


    private int _size = 1;

    public Font(string file)
    {
        _handle = SDL_ttf.TTF_OpenFont(file, 1);

        if (_handle == IntPtr.Zero)
            throw new Exception($"Could not load TTF font. SDL TTF Error: {SDL_ttf.TTF_GetError()}");
    }

    public int Size
    {
        get => _size;
        set
        {
            if (SDL_ttf.TTF_SetFontSize(_handle, value) != 0)
                throw new Exception($"Could not set font size. SDL TTF error; {SDL_ttf.TTF_GetError()}");
            _size = value;
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public Surface RenderTextSolid(string text, Color color)
    {
        return new Surface(SDL_ttf.TTF_RenderText_Solid(_handle, text,
            new SDL.SDL_Color {r = color.R, g = color.G, b = color.B, a = color.A}));
    }

    public Surface RenderTextShaded(string text, Color foregroundColor, Color backgroundColor)
    {
        return new Surface(SDL_ttf.TTF_RenderText_Shaded(_handle, text,
            new SDL.SDL_Color
                {r = foregroundColor.R, g = foregroundColor.G, b = foregroundColor.B, a = foregroundColor.A},
            new SDL.SDL_Color
                {r = foregroundColor.R, g = foregroundColor.G, b = foregroundColor.B, a = foregroundColor.A}));
    }

    public Surface RenderTextBlended(string text, Color foregroundColor)
    {
        return new Surface(SDL_ttf.TTF_RenderText_Blended(_handle, text, new SDL.SDL_Color
            {r = foregroundColor.R, g = foregroundColor.G, b = foregroundColor.B, a = foregroundColor.A}));
    }

    public Surface RenderTextBlendedWrapped(string text, Color color, uint wrapped)
    {
        return new Surface(SDL_ttf.TTF_RenderText_Blended_Wrapped(_handle, text,
            new SDL.SDL_Color {r = color.R, g = color.G, b = color.B, a = color.A}, wrapped));
    }

    public Surface RenderTextSolidWrapped(string text, Color color, uint wrapped)
    {
        return new Surface(SDL_ttf.TTF_RenderText_Solid_Wrapped(_handle, text,
            new SDL.SDL_Color {r = color.R, g = color.G, b = color.B, a = color.A}, wrapped));
    }

    public Surface RenderTextShadedWrapped(string text, Color foregroundColor, Color backgroundColor, uint wrapped)
    {
        return new Surface(SDL_ttf.TTF_RenderText_Shaded_Wrapped(_handle, text,
            new SDL.SDL_Color
                {r = foregroundColor.R, g = foregroundColor.G, b = foregroundColor.B, a = foregroundColor.A},
            new SDL.SDL_Color
                {r = foregroundColor.R, g = foregroundColor.G, b = foregroundColor.B, a = foregroundColor.A}, wrapped));
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
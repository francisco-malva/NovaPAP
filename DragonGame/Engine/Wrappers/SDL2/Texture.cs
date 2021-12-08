using System;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2;

internal class Texture : IDisposable
{
    private int _access;
    private uint _format;

    private int _height;
    private int _width;

    public Texture(Renderer renderer, uint format, int access, int w, int h)
    {
        Handle = SDL.SDL_CreateTexture(renderer.Handle, format, access, w, h);
        UpdateInformation();
    }

    public Texture(Renderer renderer, Surface surface)
    {
        Handle = SDL.SDL_CreateTextureFromSurface(renderer.Handle, surface.Handle);
        UpdateInformation();
    }

    public uint Format => _format;
    public int Access => _access;
    public int Width => _width;
    public int Height => _height;

    public IntPtr Handle { get; private set; }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public void SetBlendMode(SDL.SDL_BlendMode blendMode)
    {
        SDL.SDL_SetTextureBlendMode(Handle, blendMode);
    }

    public void SetAlphaMod(byte alpha)
    {
        SDL.SDL_SetTextureAlphaMod(Handle, alpha);
    }

    public byte GetAlphaMod()
    {
        SDL.SDL_GetTextureAlphaMod(Handle, out var result);
        return result;
    }

    public void SetColorMod(Color color)
    {
        SDL.SDL_SetTextureColorMod(Handle, color.R, color.G, color.B);
    }

    private void UpdateInformation()
    {
        var _ = SDL.SDL_QueryTexture(Handle, out _format, out _access, out _width, out _height);
    }

    private void ReleaseUnmanagedResources()
    {
        if (Handle == IntPtr.Zero) return;

        SDL.SDL_DestroyTexture(Handle);
        Handle = IntPtr.Zero;
    }

    ~Texture()
    {
        ReleaseUnmanagedResources();
    }


    public static Texture FromBmp(string bmpPath)
    {
        using var surface = new Surface(bmpPath);
        return new Texture(Game.Instance.Renderer, surface);
    }
}
using System;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Exceptions.Textures;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

internal class Texture : IDisposable
{
    public Texture(Renderer renderer, uint format, int access, int w, int h)
    {
        Handle = SDL.SDL_CreateTexture(renderer.Handle, format, access, w, h);
    }

    public Texture(Renderer renderer, Surface surface)
    {
        Handle = SDL.SDL_CreateTextureFromSurface(renderer.Handle, surface.Handle);
    }

    public IntPtr Handle { get; private set; }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public void SetBlendMode(SDL.SDL_BlendMode blendMode)
    {
        if (SDL.SDL_SetTextureBlendMode(Handle, blendMode) != 0)
            throw new TextureException($"Could not set the texture's blend mode: {SDL.SDL_GetError()}");
    }

    public void SetAlphaMod(byte alpha)
    {
        if (SDL.SDL_SetTextureAlphaMod(Handle, alpha) != 0)
            throw new TextureException(
                $"Could not set the texture's alpha modulation. SDL Error: {SDL.SDL_GetError()}");
    }

    public byte GetAlphaMod()
    {
        if (SDL.SDL_GetTextureAlphaMod(Handle, out var result) != 0)
            throw new TextureException(
                $"Could not set the texture's alpha modulation. SDL Error: {SDL.SDL_GetError()}");
        return result;
    }

    public void SetColorMod(Color color)
    {
        if (SDL.SDL_SetTextureColorMod(Handle, color.R, color.G, color.B) != 0)
            throw new TextureException($"Could not set texture's color modulation. SDL Error: {SDL.SDL_GetError()}");
    }

    public TextureInfo QueryTexture()
    {
        if (SDL.SDL_QueryTexture(Handle, out var format, out var access, out var width, out var height) != 0)
            throw new TextureException($"Could not query texture. SDL Error: {SDL.SDL_GetError()}");
        return new TextureInfo(width, height, format, access);
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
}
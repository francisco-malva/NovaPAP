#region

using System;
using DuckDuckJump.Engine.Subsystems.Files;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Exceptions.Textures;
using SDL2;
using StbImageSharp;

#endregion

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

internal class Texture : IDisposable
{
    static Texture()
    {
        White = new Texture("white");
    }

    public Texture(string path) : this(FileSystem.GetAllBytes($"Textures/{path}.png"), true)
    {
    }

    public Texture(byte[] data, bool flipped)
    {
        var result = ImageResult.FromMemory(data, ColorComponents.RedGreenBlueAlpha);
        unsafe
        {
            fixed (void* locked = result.Data)
            {
                var surface = SDL.SDL_CreateRGBSurfaceWithFormatFrom((IntPtr)locked, result.Width, result.Height, 32,
                    4 * result.Width, flipped ? SDL.SDL_PIXELFORMAT_ABGR8888 : SDL.SDL_PIXELFORMAT_RGBA8888);

                Handle = SDL.SDL_CreateTextureFromSurface(Subsystems.Graphical.Graphics.Renderer, surface);

                SDL.SDL_FreeSurface(surface);
            }
        }
    }

    public bool Invalid { get; private set; }

    public static Texture White { get; }


    public IntPtr Handle { get; private set; }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Texture()
    {
        ReleaseUnmanagedResources();
    }

    public void SetBlendMode(SDL.SDL_BlendMode blendMode)
    {
        if (Invalid)
            return;

        if (SDL.SDL_SetTextureBlendMode(Handle, blendMode) != 0)
            throw new TextureException($"Could not set the texture's blend mode: {SDL.SDL_GetError()}");
    }

    public void SetAlphaMod(byte alpha)
    {
        if (Invalid)
            return;

        if (SDL.SDL_SetTextureAlphaMod(Handle, alpha) != 0)
            throw new TextureException(
                $"Could not set the texture's alpha modulation. SDL Error: {SDL.SDL_GetError()}");
    }

    public TextureInfo QueryTexture()
    {
        if (Invalid)
            return new TextureInfo();

        if (SDL.SDL_QueryTexture(Handle, out var format, out var access, out var width, out var height) != 0)
            throw new TextureException($"Could not query texture. SDL Error: {SDL.SDL_GetError()}");
        return new TextureInfo(width, height, format, access);
    }

    private void ReleaseUnmanagedResources()
    {
        if (Invalid)
            return;

        Invalid = true;
        if (Handle == IntPtr.Zero) return;

        SDL.SDL_DestroyTexture(Handle);
        Handle = IntPtr.Zero;
    }
}
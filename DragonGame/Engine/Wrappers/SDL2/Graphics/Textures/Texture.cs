#region

using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Files;
using DuckDuckJump.Engine.Subsystems.Output;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Exceptions.Textures;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

internal class Texture : IDisposable
{
    static Texture()
    {
        White = new Texture("white");
    }

    public Texture(uint format, int access, int w, int h)
    {
        Handle = SDL.SDL_CreateTexture(Subsystems.Graphical.Graphics.Renderer, format, access, w, h);
    }

    public Texture(string path) : this(FileSystem.Open($"Textures/{path}.tex"), true)
    {
    }

    public Texture(Stream stream, bool freeStream)
    {
        var width = stream.Read<int>();
        var height = stream.Read<int>();
        
        Handle = SDL.SDL_CreateTexture(Subsystems.Graphical.Graphics.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
            (int) SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, stream.Read<int>(), stream.Read<int>());

        if (SDL.SDL_LockTexture(Handle, IntPtr.Zero, out var pixels, out var pitch) != 0)
        {
            Error.Panic("Failed to lock texture for writing.");
        }

        Span<byte> pixelData = new byte[width * height];
        stream.Read(pixelData);

        unsafe
        {
            fixed (byte* dataPtr = pixelData)
            {
                Unsafe.CopyBlock((void*) pixels, dataPtr, (uint) (width * height * sizeof(uint)));
            }
        }
        
        SDL.SDL_UnlockTexture(Handle);

        if (freeStream)
        {
            stream.Dispose();
        }
    }

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
}
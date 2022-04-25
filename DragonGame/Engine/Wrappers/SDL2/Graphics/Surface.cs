#region

using System;
using SDL2;
using StbImageSharp;

#endregion

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics;

internal class Surface : IDisposable
{
    /// <summary>
    ///     Create a surface from a decoded image.
    /// </summary>
    /// <param name="result">the decoded image.</param>
    public unsafe Surface(ImageResult result)
    {
        fixed (byte* pixels = result.Data)
        {
            Handle = SDL.SDL_CreateRGBSurfaceFrom((IntPtr) pixels,
                result.Width,
                result.Height,
                32,
                4 * result.Width,
                0x000000FF,
                0x0000FF00,
                0x00FF0000,
                0xFF000000);
        }
    }

    public Surface(IntPtr handle)
    {
        Handle = handle;
    }

    public IntPtr Handle { get; private set; }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources()
    {
        if (Handle == IntPtr.Zero) return;

        SDL.SDL_FreeSurface(Handle);
        Handle = IntPtr.Zero;
    }

    ~Surface()
    {
        ReleaseUnmanagedResources();
    }
}
using SDL2;
using StbImageSharp;
using System;

namespace DuckDuckJump.Engine.Wrappers.SDL2
{
    internal class Surface : IDisposable
    {
        public Surface(string bmpPath)
        {
            Handle = SDL.SDL_LoadBMP(bmpPath);
        }

        public unsafe Surface(ImageResult result)
        {
            fixed (byte* pixels = result.Data)
            {
                Handle = SDL.SDL_CreateRGBSurfaceFrom((IntPtr)pixels,
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
}
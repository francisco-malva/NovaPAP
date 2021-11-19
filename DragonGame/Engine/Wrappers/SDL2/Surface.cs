using System;
using SDL2;

namespace DragonGame.Engine.Wrappers.SDL2
{
    internal class Surface : IDisposable
    {
        public Surface(string bmpPath)
        {
            Handle = SDL.SDL_LoadBMP(bmpPath);
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
using System;
using SDL2;

namespace DragonGame.Wrappers
{
    internal class Renderer : IDisposable
    {
        public Renderer(Window window, int index, SDL.SDL_RendererFlags flags)
        {
            Handle = SDL.SDL_CreateRenderer(window.Handle, index, flags);
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

            SDL.SDL_DestroyRenderer(Handle);
            Handle = IntPtr.Zero;
        }

        private static unsafe void ExtractRects(int* srcPtr, int* dstPtr, Rectangle? srcRect, Rectangle? dstRect)
        {
            if (srcRect != null)
            {
                srcPtr[0] = srcRect.Value.Value.x;
                srcPtr[1] = srcRect.Value.Value.y;
                srcPtr[2] = srcRect.Value.Value.w;
                srcPtr[3] = srcRect.Value.Value.h;
            }

            if (dstRect == null) return;
            dstPtr[0] = dstRect.Value.Value.x;
            dstPtr[1] = dstRect.Value.Value.y;
            dstPtr[2] = dstRect.Value.Value.w;
            dstPtr[3] = dstRect.Value.Value.h;
        }

        public void Copy(Texture texture, Rectangle? srcRect, Rectangle? dstRect)
        {
            unsafe
            {
                var srcBuffer = stackalloc int[4];
                var dstBuffer = stackalloc int[4];

                ExtractRects(srcBuffer, dstBuffer, srcRect, dstRect);

                var _ = SDL.SDL_RenderCopy(Handle, texture.Handle,
                    srcRect == null ? IntPtr.Zero : new IntPtr(srcBuffer),
                    dstRect == null ? IntPtr.Zero : new IntPtr(dstBuffer));
            }
        }

        public void SetTarget(Texture texture)
        {
            var _ = SDL.SDL_SetRenderTarget(Handle, texture?.Handle ?? IntPtr.Zero);
        }

        public void Clear()
        {
            var _ = SDL.SDL_RenderClear(Handle);
        }

        public void Present()
        {
            SDL.SDL_RenderPresent(Handle);
        }

        public void SetDrawColor(byte r, byte g, byte b, byte a)
        {
            var _ = SDL.SDL_SetRenderDrawColor(Handle, r, g, b, a);
        }

        public void DrawLine(ref Point a, ref Point b)
        {
            var _ = SDL.SDL_RenderDrawLine(Handle, a.X, a.Y, b.X, b.Y);
        }

        public void SetScale(ref Point scale)
        {
            var _ = SDL.SDL_RenderSetScale(Handle, scale.X, scale.Y);
        }

        ~Renderer()
        {
            ReleaseUnmanagedResources();
        }
    }
}
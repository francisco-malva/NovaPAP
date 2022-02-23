using System;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Exceptions.Renderer;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics;

/// <summary>
///     An abstraction to the SDL renderer class.
/// </summary>
internal class Renderer : IDisposable
{
    public Renderer(Window window, int index, SDL.SDL_RendererFlags flags)
    {
        Handle = SDL.SDL_CreateRenderer(window.Handle, index, flags);
        if (Handle == IntPtr.Zero)
            throw new RendererCreationException($"Failed to create the renderer. SDL Error: {SDL.SDL_GetError()}");
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

    /// <summary>
    ///     Transfer the managed representation of a rectangle to an arbitrary point in memory.
    /// </summary>
    /// <param name="managed">The managed rectangle structure.</param>
    /// <param name="unmanaged">The destination pointer for the data.</param>
    private static unsafe void TransferManagedRectangle(Rectangle? managed, int* unmanaged)
    {
        if (managed == null) return;
        unmanaged[0] = managed.Value.Value.x;
        unmanaged[1] = managed.Value.Value.y;
        unmanaged[2] = managed.Value.Value.w;
        unmanaged[3] = managed.Value.Value.h;
    }

    /// <summary>
    ///     Transfer managed rectangle structs to native memory, useful for SDL interoperability.
    /// </summary>
    /// <param name="srcRect">The managed representation of the source rectangle.</param>
    /// <param name="dstRect">The managed representation of the destination rectangle.</param>
    /// <param name="srcPtr">Pointer to the unmanaged representation of the source rectangle.</param>
    /// <param name="dstPtr">Pointer to the unmanaged representation of the destination rectangle.</param>
    private static unsafe void TransferManagedDrawingRectangles(Rectangle? srcRect, Rectangle? dstRect, int* srcPtr,
        int* dstPtr)
    {
        TransferManagedRectangle(srcRect, srcPtr);
        TransferManagedRectangle(dstRect, dstPtr);
    }

    /// <summary>
    ///     Copy a texture to the renderer's target.
    /// </summary>
    /// <param name="texture">The texture to copy.</param>
    /// <param name="srcRect">The source rectangle. If left null, will copy the entire source region.</param>
    /// <param name="dstRect">The destination rectangle. If left null, will copy to the entire target area.</param>
    public void Copy(Texture texture, Rectangle? srcRect, Rectangle? dstRect)
    {
        unsafe
        {
            var unmanagedSourceRectangle = stackalloc int[4];
            var unmanagedDestinationRectangle = stackalloc int[4];

            TransferManagedDrawingRectangles(srcRect, dstRect, unmanagedSourceRectangle, unmanagedDestinationRectangle);

            var _ = SDL.SDL_RenderCopy(Handle, texture.Handle,
                srcRect == null ? IntPtr.Zero : new IntPtr(unmanagedSourceRectangle),
                dstRect == null ? IntPtr.Zero : new IntPtr(unmanagedDestinationRectangle));
        }
    }

    public void SetRenderTarget(Texture texture)
    {
        var _ = SDL.SDL_SetRenderTarget(Handle, texture?.Handle ?? IntPtr.Zero);
    }

    public void Clear()
    {
        if (SDL.SDL_RenderClear(Handle) != 0)
            throw new RendererOperationException(
                $"Failed to clear the renderer's framebuffer color. SDL Error: {SDL.SDL_GetError()}");
    }

    public void Present()
    {
        SDL.SDL_RenderPresent(Handle);
    }

    public void SetDrawColor(Color color)
    {
        if (SDL.SDL_SetRenderDrawColor(Handle, color.R, color.G, color.B, color.A) != 0)
            throw new RendererOperationException(
                $"Failed to set the renderer's draw color. SDL Error: {SDL.SDL_GetError()}");
    }

    public void DrawLine(Point a, Point b)
    {
        if (SDL.SDL_RenderDrawLine(Handle, a.X, a.Y, b.X, b.Y) != 0)
            throw new RendererOperationException($"Failed to draw a line. SDL Error: {SDL.SDL_GetError()}");
    }

    public void CopyEx(Texture texture, Rectangle? source, Rectangle? dest, double angle, Point? center,
        SDL.SDL_RendererFlip flip)
    {
        unsafe
        {
            //The unmanaged storage for the source rectangle.
            var unmanagedSourceRectangle = stackalloc int[4];
            //The unmanaged storage for the destination rectangle.
            var unmanagedDestinationRectangle = stackalloc int[4];
            //The unmanaged storage for the center point.
            var unmanagedCenterPoint = stackalloc int[2];

            TransferManagedDrawingRectangles(source, dest, unmanagedSourceRectangle, unmanagedDestinationRectangle);

            if (center != null)
            {
                unmanagedCenterPoint[0] = center.Value.X;
                unmanagedCenterPoint[1] = center.Value.Y;
            }

            var _ = SDL.SDL_RenderCopyEx(Handle,
                texture.Handle,
                source == null ? IntPtr.Zero : new IntPtr(unmanagedSourceRectangle),
                dest == null ? IntPtr.Zero : new IntPtr(unmanagedDestinationRectangle),
                angle,
                center == null ? IntPtr.Zero : new IntPtr(unmanagedCenterPoint),
                flip);
        }
    }

    public void SetLogicalSize(Point size)
    {
        if (SDL.SDL_RenderSetLogicalSize(Handle, size.X, size.Y) != 0)
            throw new RendererOperationException(
                $"Failed to set the renderer's logical size. SDL Error: {SDL.SDL_GetError()}");
    }

    ~Renderer()
    {
        ReleaseUnmanagedResources();
    }
}
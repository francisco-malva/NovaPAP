#region

using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using DuckDuckJump.Engine.Subsystems.Output;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Configuration;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Graphical;

internal static class Graphics
{
    public static Camera Camera;

    public static readonly Size LogicalSize = new(640, 480);
    public static readonly Vector2 Midpoint = new(LogicalSize.Width / 2.0f, LogicalSize.Height / 2.0f);

    private static readonly SDL.SDL_Vertex[] Vertices = new SDL.SDL_Vertex[4];
    private static readonly int[] Indices = { 0, 1, 2, 1, 3, 2 };

    public static IntPtr Window { get; private set; }

    public static IntPtr Renderer { get; private set; }

    public static Rectangle? Viewport
    {
        set
        {
            unsafe
            {
                int result;
                if (!value.HasValue)
                {
                    result = SDL_RenderSetViewport(Renderer, null);
                }
                else
                {
                    var rect = new SDL.SDL_Rect
                        { x = value.Value.X, y = value.Value.Y, w = value.Value.Width, h = value.Value.Height };
                    result = SDL_RenderSetViewport(Renderer, &rect);
                }

                if (result != 0)
                {
                }
            }
        }
    }

    public static void SetFullscreen()
    {
        SDL.SDL_SetWindowFullscreen(Window,
            (uint)(Settings.MyData.Fullscreen ? SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN : 0));
    }

    public static void Initialize()
    {
        Window = SDL.SDL_CreateWindow("Duck Duck Jump!", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 640,
            480, 0);


        if (Window == IntPtr.Zero)
            Error.RaiseMessage("Could not create window.");

        Renderer = SDL.SDL_CreateRenderer(Window, -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);

        if (Renderer == IntPtr.Zero)
            Error.RaiseMessage($"Could not create window and renderer. SDL Error: {SDL.SDL_GetError()}");

        if (SDL.SDL_SetRenderDrawBlendMode(Renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND) != 0)
            Error.RaiseMessage($"Could not set correct blending mode. SDL Error: {SDL.SDL_GetError()}");

        SetFullscreen();
    }

    public static void Quit()
    {
        SDL.SDL_DestroyRenderer(Renderer);
        SDL.SDL_DestroyWindow(Window);
        Renderer = IntPtr.Zero;
        Window = IntPtr.Zero;
    }

    public static void Begin()
    {
        SDL.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 255);
        SDL.SDL_RenderClear(Renderer);
    }

    public static void Draw(Texture texture, RectangleF? source, Matrix3x2 transformation, Color color)
    {
        var textureInfo = texture.QueryTexture();

        RectangleF positionRectangle, textureRectangle;

        if (!source.HasValue)
        {
            positionRectangle = new RectangleF(0.0f, 0.0f, textureInfo.Width, textureInfo.Height);
            textureRectangle = new RectangleF(0.0f, 0.0f, 1.0f, 1.0f);
        }
        else
        {
            positionRectangle = new RectangleF(0.0f, 0.0f, source.Value.Width, source.Value.Height);
            textureRectangle = new RectangleF(source.Value.X / textureInfo.Width,
                source.Value.Y / textureInfo.Height, source.Value.Width / textureInfo.Width,
                source.Value.Height / textureInfo.Height);
        }

        var concatenatedTransform = Camera == null ? transformation : Camera.Matrix * transformation;

        var topLeft = Vector2.Transform(new Vector2(positionRectangle.Left, positionRectangle.Top),
            concatenatedTransform);
        var topRight = Vector2.Transform(new Vector2(positionRectangle.Right, positionRectangle.Top),
            concatenatedTransform);
        var bottomLeft = Vector2.Transform(new Vector2(positionRectangle.Left, positionRectangle.Bottom),
            concatenatedTransform);
        var bottomRight = Vector2.Transform(new Vector2(positionRectangle.Right, positionRectangle.Bottom),
            concatenatedTransform);

        Vertices[0].position.x = topLeft.X;
        Vertices[0].position.y = topLeft.Y;
        Vertices[0].tex_coord.x = textureRectangle.Left;
        Vertices[0].tex_coord.y = textureRectangle.Top;

        Vertices[1].position.x = topRight.X;
        Vertices[1].position.y = topRight.Y;
        Vertices[1].tex_coord.x = textureRectangle.Right;
        Vertices[1].tex_coord.y = textureRectangle.Top;

        Vertices[2].position.x = bottomLeft.X;
        Vertices[2].position.y = bottomLeft.Y;
        Vertices[2].tex_coord.x = textureRectangle.Left;
        Vertices[2].tex_coord.y = textureRectangle.Bottom;

        Vertices[3].position.x = bottomRight.X;
        Vertices[3].position.y = bottomRight.Y;
        Vertices[3].tex_coord.x = textureRectangle.Right;
        Vertices[3].tex_coord.y = textureRectangle.Bottom;

        var sdlColor = new SDL.SDL_Color { r = color.R, g = color.G, b = color.B, a = color.A };

        for (var i = 0; i < Vertices.Length; i++)
            Vertices[i].color = sdlColor;

        if (SDL.SDL_RenderGeometry(Renderer, texture.Handle, Vertices, Vertices.Length, Indices, Indices.Length) != 0)
        {
#if DEBUG
            Console.WriteLine($"Cannot draw object. SDL Error: {SDL.SDL_GetError()}");
#endif
        }
    }

    public static void End()
    {
        SDL.SDL_RenderPresent(Renderer);
    }

    [DllImport("SDL2")]
    private static extern unsafe int SDL_RenderSetViewport(IntPtr renderer, SDL.SDL_Rect* rect);
}
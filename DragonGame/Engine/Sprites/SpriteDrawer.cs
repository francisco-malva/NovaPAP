using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using SDL2;

namespace DuckDuckJump.Engine.Sprites;

internal class SpriteDrawer
{
    private static readonly SDL.SDL_Vertex[] Vertices = new SDL.SDL_Vertex[4];
    private static readonly int[] Indices = { 0, 1, 2, 1, 3, 2 };
    private readonly Stack<Matrix3x2> _matrixStack = new();

    private readonly Renderer _renderer;

    private Matrix3x2 _matrix = Matrix3x2.Identity;

    public SpriteDrawer(Renderer renderer)
    {
        _renderer = renderer;
    }

    public void PushMatrix()
    {
        _matrixStack.Push(_matrix);
    }

    public void PopMatrix()
    {
        _matrix = _matrixStack.Count == 0 ? Matrix3x2.Identity : _matrixStack.Pop();
    }

    public void MultiplyMatrix(Matrix3x2 matrix)
    {
        _matrix *= matrix;
    }

    public void Translate(float x, float y)
    {
        _matrix *= Matrix3x2.CreateTranslation(x, y);
    }


    public void Scale(float x, float y)
    {
        _matrix *= Matrix3x2.CreateScale(x, y);
    }

    public void Scale(float x, float y, Vector2 centerPoint)
    {
        _matrix *= Matrix3x2.CreateScale(x, y, centerPoint);
    }

    public void Rotate(float angle)
    {
        _matrix *= Matrix3x2.CreateRotation(angle);
    }

    public void Rotate(float angle, Vector2 centerPoint)
    {
        _matrix *= Matrix3x2.CreateRotation(angle, centerPoint);
    }

    public void Skew(float x, float y)
    {
        _matrix *= Matrix3x2.CreateSkew(x, y);
    }

    public void Skew(float x, float y, Vector2 centerPoint)
    {
        _matrix *= Matrix3x2.CreateSkew(x, y, centerPoint);
    }

    public void Draw(Texture texture, RectangleF? rectangle = null)
    {
        Draw(texture, Color.White, rectangle);
    }

    public void Draw(Texture texture, Color color, RectangleF? rectangle)
    {
        var textureInfo = texture.QueryTexture();

        RectangleF positionRectangle, textureRectangle;

        if (rectangle == null)
        {
            positionRectangle = new RectangleF(0.0f, 0.0f, textureInfo.Width, textureInfo.Height);
            textureRectangle = new RectangleF(0.0f, 0.0f, 1.0f, 1.0f);
        }
        else
        {
            positionRectangle = new RectangleF(0.0f, 0.0f, rectangle.Value.Width, rectangle.Value.Height);
            textureRectangle = new RectangleF(rectangle.Value.X / textureInfo.Width,
                rectangle.Value.Y / textureInfo.Height, rectangle.Value.Width / textureInfo.Width,
                rectangle.Value.Height / textureInfo.Height);
        }


        var topLeft = Vector2.Transform(new Vector2(positionRectangle.Left, positionRectangle.Top), _matrix);
        var topRight = Vector2.Transform(new Vector2(positionRectangle.Right, positionRectangle.Top), _matrix);
        var bottomLeft = Vector2.Transform(new Vector2(positionRectangle.Left, positionRectangle.Bottom), _matrix);
        var bottomRight = Vector2.Transform(new Vector2(positionRectangle.Right, positionRectangle.Bottom), _matrix);

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

        for (var i = 0; i < Vertices.Length; i++)
            Vertices[i].color = new SDL.SDL_Color { r = color.R, g = color.G, b = color.B, a = color.A };

        _renderer.RenderGeometry(texture, Vertices, Vertices.Length, Indices, Indices.Length);
    }
}
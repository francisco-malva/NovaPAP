using System.Drawing;
using DuckDuckJump.Engine.Utilities;

namespace DuckDuckJump.Game.Gameplay;

internal class Camera
{
    private readonly Rectangle _limits;
    private readonly Point _viewport;


    private Point _position;

    public Camera(Point viewport, Rectangle limits)
    {
        _viewport = viewport;
        _limits = limits;
    }

    public Point Position
    {
        get => _position;
        set
        {
            _position.X = Mathematics.Clamp(value.X, _limits.Left, _limits.Right);
            _position.Y = Mathematics.Clamp(value.Y, _limits.Top, _limits.Bottom);
        }
    }

    public bool OnScreen(Rectangle rectangle)
    {
        var viewport = new Rectangle(0, 0, _viewport.X, _viewport.Y);
        return rectangle.IntersectsWith(viewport);
    }

    /// <summary>
    ///     Transforms a point from the world space to the view space.
    /// </summary>
    public Point TransformPoint(Point point)
    {
        return new Point(point.X - Position.X, _viewport.Y - point.Y + Position.Y);
    }
}
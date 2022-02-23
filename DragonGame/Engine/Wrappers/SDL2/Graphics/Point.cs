using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics;

/// <summary>
///     A wrapper around an SDL point.
/// </summary>
public struct Point
{
    /// <summary>
    ///     The underlying value.
    /// </summary>
    public SDL.SDL_Point Value;

    /// <summary>
    ///     A constant that holds the value { X = 1, Y = 1 }.
    /// </summary>
    public static readonly Point One = new(1, 1);

    /// <summary>
    ///     The X position.
    /// </summary>
    public int X
    {
        get => Value.x;
        set => Value.x = value;
    }

    /// <summary>
    ///     The Y position.
    /// </summary>
    public int Y
    {
        get => Value.y;
        set => Value.y = value;
    }

    public Point(int x, int y)
    {
        Value = new SDL.SDL_Point {x = x, y = y};
    }
}
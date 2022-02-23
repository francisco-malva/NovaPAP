using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics;

public struct Color
{
    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color Red = new(255, 0, 0);
    public static readonly Color Blue = new(0, 0, 255);
    public static readonly Color Green = new(0, 255, 0);
    public static readonly Color Yellow = new(255, 255, 0);

    public readonly SDL.SDL_Color Value;

    /// <summary>
    ///     The red channel.
    /// </summary>
    public byte R => Value.r;

    /// <summary>
    ///     The green channel.
    /// </summary>
    public byte G => Value.g;

    /// <summary>
    ///     The blue channel.
    /// </summary>
    public byte B => Value.b;

    /// <summary>
    ///     The alpha channel.
    /// </summary>
    public byte A => Value.a;


    public Color(byte r, byte g, byte b, byte a = 255)
    {
        Value = new SDL.SDL_Color {r = r, g = g, b = b, a = a};
    }
}
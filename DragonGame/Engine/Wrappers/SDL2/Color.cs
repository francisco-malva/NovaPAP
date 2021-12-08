namespace DuckDuckJump.Engine.Wrappers.SDL2;

internal struct Color
{
    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color Red = new(255, 0, 0);
    public static readonly Color Blue = new(0, 0, 255);
    public static readonly Color Green = new(0, 255, 0);
    public static readonly Color Yellow = new(255, 255, 0);

    /// <summary>
    ///     The red channel.
    /// </summary>
    public byte R;

    /// <summary>
    ///     The green channel.
    /// </summary>
    public byte G;

    /// <summary>
    ///     The blue channel.
    /// </summary>
    public byte B;

    /// <summary>
    ///     The alpha channel.
    /// </summary>
    public byte A;


    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}
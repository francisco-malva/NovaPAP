namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

internal readonly struct TextureInfo
{
    public readonly int Width;
    public readonly int Height;
    public readonly uint Format;
    public readonly int Access;

    public TextureInfo(int width, int height, uint format, int access)
    {
        Width = width;
        Height = height;
        Format = format;
        Access = access;
    }
}
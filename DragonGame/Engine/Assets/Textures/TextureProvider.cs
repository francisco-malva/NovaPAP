using System.IO;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using StbImageSharp;

namespace DuckDuckJump.Engine.Assets.Textures;

/// <summary>
///     A class that handles the loading and caching of textures.
/// </summary>
internal class TextureProvider : ResourceProvider<Texture>
{
    /// <summary>
    ///     Handle to the renderer that is to be used to load textures.
    /// </summary>
    private readonly Renderer _renderer;

    public TextureProvider(Renderer renderer) : base("Textures", "png")
    {
        _renderer = renderer;
    }

    protected override Texture LoadAsset(string path)
    {
        using var file = File.OpenRead(path);
        using var surface = new Surface(ImageResult.FromStream(file, ColorComponents.RedGreenBlueAlpha));
        return new Texture(_renderer, surface);
    }
}
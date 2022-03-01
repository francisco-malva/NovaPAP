using System;
using DuckDuckJump.Engine.Assets.Providers.Audio;
using DuckDuckJump.Engine.Assets.Providers.Fonts;
using DuckDuckJump.Engine.Assets.Providers.Textures;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;

namespace DuckDuckJump.Engine.Assets;

internal class ResourceManager : IDisposable
{
    public readonly ChunkProvider Chunks;
    public readonly FontProvider Fonts;
    public readonly MusicProvider Musics;
    public readonly TextureProvider Textures;

    public ResourceManager(Renderer renderer)
    {
        Textures = new TextureProvider(renderer);
        Chunks = new ChunkProvider();
        Musics = new MusicProvider();
        Fonts = new FontProvider();
    }

    public void Dispose()
    {
        Textures.Dispose();
        Chunks.Dispose();
        Musics.Dispose();
        Fonts.Dispose();
    }
}
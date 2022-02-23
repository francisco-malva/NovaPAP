using System;
using DuckDuckJump.Engine.Assets.Audio;
using DuckDuckJump.Engine.Assets.Textures;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;

namespace DuckDuckJump.Engine.Assets;

internal class ResourceProviders : IDisposable
{
    public readonly ChunkProvider Chunks;
    public readonly MusicProvider Musics;
    public readonly TextureProvider Textures;

    public ResourceProviders(Renderer renderer)
    {
        Textures = new TextureProvider(renderer);
        Chunks = new ChunkProvider();
        Musics = new MusicProvider();
    }

    public void Dispose()
    {
        Textures.Dispose();
        Chunks.Dispose();
        Musics.Dispose();
    }
}
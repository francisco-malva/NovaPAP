using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

namespace DuckDuckJump.Engine.Assets.Audio;

/// <summary>
///     A class that handles loading and caching audio chunks.
/// </summary>
internal class ChunkProvider : ResourceProvider<Chunk>
{
    public ChunkProvider() : base("Audio", "ogg")
    {
    }

    protected override Chunk LoadAsset(string path)
    {
        return new Chunk(path);
    }
}
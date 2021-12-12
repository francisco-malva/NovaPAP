using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

namespace DuckDuckJump.Engine.Assets.Audio;

internal class ChunkManager : ResourceManager<Chunk>
{
    public ChunkManager() : base("Assets/Audio", "ogg")
    {
    }

    protected override Chunk LoadAsset(string path)
    {
        return new Chunk(path);
    }
}
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

namespace DuckDuckJump.Engine.Assets.Audio;

internal class MusicManager : ResourceManager<Music>
{
    public MusicManager() : base("Assets/Music", "ogg")
    {
    }

    protected override Music LoadAsset(string path)
    {
        return new Music(path);
    }
}
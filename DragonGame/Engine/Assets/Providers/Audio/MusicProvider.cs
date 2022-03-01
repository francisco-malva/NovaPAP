using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

namespace DuckDuckJump.Engine.Assets.Providers.Audio;

/// <summary>
///     A class that handles the loading and caching of music.
/// </summary>
internal class MusicProvider : ResourceProvider<Music>
{
    public MusicProvider() : base("Music", "ogg")
    {
    }

    protected override Music LoadAsset(string path)
    {
        return new Music(path);
    }
}
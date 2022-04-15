using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;
using DuckDuckJump.Game.Gameplay.Resources;

namespace DuckDuckJump.Game.Gameplay.Announcer;

internal class Announcer
{
    private readonly Chunk[] _clips;

    public Announcer(GameplayResources resources)
    {
        _clips = resources.AnnouncerClips;
    }

    public void Say(AnnouncementType announcement)
    {
        _ = _clips[(int)announcement].Play(-1, 0);
    }
}
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

namespace DuckDuckJump.Scenes.Game.Gameplay.Announcer;

internal class Announcer
{
    private readonly Chunk[] _clips =
    {
        Engine.Game.Instance.ChunkManager["get-ready"],
        Engine.Game.Instance.ChunkManager["go"],
        Engine.Game.Instance.ChunkManager["p1-wins"],
        Engine.Game.Instance.ChunkManager["p2-wins"],
        Engine.Game.Instance.ChunkManager["draw"]
    };

    public void Say(AnnouncementType announcement)
    {
        _ = _clips[(int)announcement].Play(-1, 0);
    }
}
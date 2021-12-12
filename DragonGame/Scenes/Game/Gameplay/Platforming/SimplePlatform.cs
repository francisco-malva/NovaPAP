using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Players;

namespace DuckDuckJump.Scenes.Game.Gameplay.Platforming;

internal class SimplePlatform : Platform
{
    public SimplePlatform(ushort id, Point position, Player player) : base(id, position, player)
    {
    }

    protected override void OnPlayerJump()
    {
    }

    protected override Color GetPlatformDrawColor()
    {
        return new Color(208, 227, 196);
    }

    protected override void OnUpdate()
    {
    }

    public override bool TargetableByAi()
    {
        return true;
    }
}
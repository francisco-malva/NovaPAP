using System.Drawing;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

namespace DuckDuckJump.Game.Gameplay.Platforming;

internal class SimplePlatform : Platform
{
    public SimplePlatform(Point position, Texture? platformTexture) : base(position,
        platformTexture)
    {
    }

    protected override void OnPlayerJump()
    {
    }

    protected override Color GetPlatformDrawColor()
    {
        return Color.FromArgb(208, 227, 196);
    }

    protected override void OnUpdate()
    {
    }

    public override bool CanBeTargetedByAi()
    {
        return true;
    }
}
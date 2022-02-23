using System;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

namespace DuckDuckJump.Scenes.Game.Gameplay.Platforming;

internal class MovingPlatform : Platform
{
    private const int PlatformMoveSpeed = 3;
    private bool _moveLeft;

    public MovingPlatform(Point position, Random random, Texture platformTexture) : base(position, platformTexture)
    {
        _moveLeft = random.NextSingle() >= 0.5f;
    }

    protected override void OnPlayerJump()
    {
    }

    protected override Color GetPlatformDrawColor()
    {
        return new Color(173, 198, 152);
    }

    protected override void OnUpdate()
    {
        if (_moveLeft)
        {
            Position.X -= PlatformMoveSpeed;

            if (Position.X <= PlatformWidth / 2) _moveLeft = false;
        }
        else
        {
            Position.X += PlatformMoveSpeed;

            if (Position.X >= GameField.Width - PlatformWidth / 2) _moveLeft = true;
        }
    }

    public override bool CanBeTargetedByAi()
    {
        return true;
    }
}
using System;
using System.Drawing;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Gameplay.Players;

namespace DuckDuckJump.Game.Gameplay.Platforming;

internal class TeleportingPlatform : Platform
{
    private const ushort StaticTime = 180;
    private const ushort DissapearingTime = 30;
    private const ushort AppearingTime = 30;

    private readonly Random _random;
    private TeleportingPlatformState _state;

    private ushort _stateTimer;

    public TeleportingPlatform(Point position, Random random, Texture? platformTexture) : base(position,
        platformTexture)
    {
        _random = random;
        SetState(TeleportingPlatformState.Static);
        _stateTimer = (ushort) _random.Next(10, StaticTime);
    }

    protected override bool ShouldPlayerTriggerJump(Player player)
    {
        return base.ShouldPlayerTriggerJump(player) && _state == TeleportingPlatformState.Static;
    }

    protected override void OnPlayerJump()
    {
    }

    protected override Color GetPlatformDrawColor()
    {
        switch (_state)
        {
            case TeleportingPlatformState.Static:
                var color = (byte) ((float) _stateTimer / StaticTime * 255.0f);
                return Color.FromArgb(color, color, color);
            case TeleportingPlatformState.Dissapearing:
                return Color.Black;
            case TeleportingPlatformState.Appearing:
                return Color.White;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void OnUpdate()
    {
        switch (_state)
        {
            case TeleportingPlatformState.Static:
                Alpha = byte.MaxValue;
                if (_stateTimer == 0) SetState(TeleportingPlatformState.Dissapearing);
                break;
            case TeleportingPlatformState.Dissapearing:
                Alpha = (byte) (_stateTimer / (float) DissapearingTime * 255.0f);
                if (_stateTimer == 0)
                {
                    Position.X = _random.Next(PlatformWidth / 2, GameField.Width - PlatformWidth / 2);
                    SetState(TeleportingPlatformState.Appearing);
                }

                break;
            case TeleportingPlatformState.Appearing:
                Alpha = (byte) ((1.0f - (float) _stateTimer / DissapearingTime) * 255);
                if (_stateTimer == 0) SetState(TeleportingPlatformState.Static);
                break;
        }

        if (_stateTimer > 0) --_stateTimer;
    }

    private void SetState(TeleportingPlatformState state)
    {
        _state = state;
        switch (state)
        {
            case TeleportingPlatformState.Static:
                _stateTimer = StaticTime;
                break;
            case TeleportingPlatformState.Dissapearing:
                _stateTimer = DissapearingTime;
                break;
            case TeleportingPlatformState.Appearing:
                _stateTimer = AppearingTime;
                break;
        }
    }

    public override bool CanBeTargetedByAi()
    {
        return _state == TeleportingPlatformState.Static;
    }
}
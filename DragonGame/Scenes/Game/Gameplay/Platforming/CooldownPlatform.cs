using System;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Scenes.Game.Gameplay.Players;

namespace DuckDuckJump.Scenes.Game.Gameplay.Platforming;

internal sealed class CooldownPlatform : Platform
{
    private const ushort FadeOutTime = 15;
    private const ushort FadeInTime = 15;
    private const ushort GoneTime = 60;

    private CooldownPlatformState _state;

    private ushort _timer;

    public CooldownPlatform(Point position, Texture platformTexture) : base(position,
        platformTexture)
    {
        SetState(CooldownPlatformState.Static);
    }

    private bool ShouldSwitchState => _timer == 0;

    private void SetState(CooldownPlatformState state)
    {
        _state = state;
        switch (state)
        {
            case CooldownPlatformState.Static:
                _timer = 0;
                break;
            case CooldownPlatformState.FadingOut:
                _timer = FadeOutTime;
                break;
            case CooldownPlatformState.FadingIn:
                _timer = FadeInTime;
                break;
            case CooldownPlatformState.Gone:
                _timer = GoneTime;
                break;
        }
    }

    protected override void OnPlayerJump()
    {
        SetState(CooldownPlatformState.FadingOut);
    }

    protected override Color GetPlatformDrawColor()
    {
        return new Color(192, 87, 70);
    }

    protected override void OnUpdate()
    {
        switch (_state)
        {
            case CooldownPlatformState.Static:
                break;
            case CooldownPlatformState.FadingOut:
                FadingOutUpdate();
                break;
            case CooldownPlatformState.FadingIn:
                FadingInUpdate();
                break;
            case CooldownPlatformState.Gone:
                GoneUpdate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (_timer > 0) --_timer;
    }

    private void GoneUpdate()
    {
        if (ShouldSwitchState) SetState(CooldownPlatformState.FadingIn);
    }

    private void FadingOutUpdate()
    {
        Alpha = (byte) (_timer / (float) FadeOutTime * 255.0f);
        if (ShouldSwitchState) SetState(CooldownPlatformState.Gone);
    }

    private void FadingInUpdate()
    {
        Alpha = (byte) ((1.0f - _timer / (float) FadeOutTime) * 255.0f);
        if (ShouldSwitchState) SetState(CooldownPlatformState.Static);
    }

    protected override bool ShouldPlayerTriggerJump(Player player)
    {
        return base.ShouldPlayerTriggerJump(player) && _state == CooldownPlatformState.Static;
    }

    public override bool CanBeTargetedByAi()
    {
        return _state == CooldownPlatformState.Static;
    }
}
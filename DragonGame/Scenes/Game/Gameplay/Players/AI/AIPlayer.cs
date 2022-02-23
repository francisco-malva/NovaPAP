using System;
using DuckDuckJump.Scenes.Game.Gameplay.Items;
using DuckDuckJump.Scenes.Game.Gameplay.Platforming;
using DuckDuckJump.Scenes.Game.Gameplay.Resources;
using DuckDuckJump.Scenes.Game.Input;

namespace DuckDuckJump.Scenes.Game.Gameplay.Players.AI;

internal class AiPlayer : Player
{
    private readonly AiDifficulty _difficulty;
    private AiState _state;
    private Platform? _target;
    private ushort _timer;

    public AiPlayer(AiDifficulty difficulty, Random random, GameplayResources resources, ItemManager manager) : base(
        random, resources, manager)
    {
        _difficulty = difficulty;
    }

    private ushort WaitTime
    {
        get
        {
            return _difficulty switch
            {
                AiDifficulty.Easy => 60,
                AiDifficulty.Normal => 30,
                AiDifficulty.Hard => 25,
                AiDifficulty.Nightmare => 0,
                _ => 0
            };
        }
    }

    protected override void MoveX(PlatformManager platformManager, GameInput input)
    {
        switch (_state)
        {
            case AiState.Waiting:
                SteerAi();
                break;
            case AiState.SelectingPlatform:
                ItemManager?.UseItem();
                var newTarget = platformManager.GetAiTarget(this);

                if (newTarget != null) _target = newTarget;
                SetAiState(AiState.Waiting);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (_timer > 0) --_timer;
    }

    private void SteerAi()
    {
        if (_target == null || !_target.CanBeTargetedByAi() || _target.InZone(this))
        {
            XSpeed = 0;
            return;
        }

        var sign = Math.Sign(_target.Position.X - Position.X);

        XSpeed = sign * XMoveSpeed;
    }

    private void SetAiState(AiState state)
    {
        _state = state;
        switch (state)
        {
            case AiState.Waiting:
                _timer = WaitTime;
                break;
            case AiState.SelectingPlatform:
                break;
        }
    }

    protected override void OnJump(Platform? platform)
    {
        if (_timer == 0) SetAiState(AiState.SelectingPlatform);
    }

    protected override void ResetSpecialFields()
    {
        _target = null;
        SetAiState(AiState.SelectingPlatform);
    }

    protected override void OnPressSpecial()
    {
    }
}
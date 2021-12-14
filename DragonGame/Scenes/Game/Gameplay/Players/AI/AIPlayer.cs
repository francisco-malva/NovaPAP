using System;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Scenes.Game.Gameplay.Platforming;
using DuckDuckJump.Scenes.Game.Input;

namespace DuckDuckJump.Scenes.Game.Gameplay.Players.AI;

internal class AIPlayer : Player
{
    private readonly AiDifficulty _difficulty;
    private AiState _state;
    private Platform _target;
    private ushort _timer;

    public AIPlayer(AiDifficulty difficulty, DeterministicRandom random) : base(random)
    {
        _difficulty = difficulty;
    }

    private ushort WaitTime
    {
        get
        {
            switch (_difficulty)
            {
                case AiDifficulty.Easy:
                    return 60;
                case AiDifficulty.Normal:
                    return 30;
                case AiDifficulty.Hard:
                    return 25;
                case AiDifficulty.Nightmare:
                    return 0;
            }

            return 0;
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
        if (_target == null || !_target.TargetableByAi() || _target.InZone(this))
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

    protected override void OnJump(Platform platform)
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
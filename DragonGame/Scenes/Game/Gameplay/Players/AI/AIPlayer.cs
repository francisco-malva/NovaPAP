using System;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Input;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;

namespace DragonGame.Scenes.Game.Gameplay.Players.AI
{
    internal class AIPlayer : Player
    {
        private readonly AiDifficulty _difficulty;
        private AiState _state;
        private ushort _timer;
        private Platform _target;

        public AIPlayer(AiDifficulty difficulty, DeterministicRandom random) : base(random)
        {
            _difficulty = difficulty;
        }

        protected override void MoveX(Platforms platforms, GameInput input)
        {
            switch (_state)
            {
                case AiState.Waiting:
                    SteerAi();
                    break;
                case AiState.SelectingPlatform:
                    var newTarget = platforms.GetAiTarget(this);

                    if (newTarget != null)
                    {
                        _target = newTarget;
                    }
                    SetAiState(AiState.Waiting);
                    break;
            }
            if (_timer > 0)
            {
                --_timer;
            }
        }

        private void SteerAi()
        {
            if (_target == null || !_target.TargetableByAi() || _target.InZone(this))
            {
                XSpeed = 0;
                return;
            }

            int sign = Math.Sign(_target.Position.X - Position.X);

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

        protected override void OnJump(Platform platform)
        {
            if (_timer == 0)
            {
                SetAiState(AiState.SelectingPlatform);
            }
        }

        protected override void ResetSpecialFields()
        {
            _target = null;
            SetAiState(AiState.SelectingPlatform);
        }
    }
}
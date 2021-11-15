using System;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Players.AI
{
    internal class AIPlayer : Player
    {
        private readonly AiDifficulty _difficulty;
        private bool _canFailTarget = true;

        private byte _failTimer;
        private short _platformTarget = -1;
        private int _prevXDiff;
        private bool _selectTarget = true;

        public AIPlayer(AiDifficulty difficulty, DeterministicRandom random, Texture texture) : base(random, texture)
        {
            _difficulty = difficulty;
        }

        protected override void MoveX(Platforms platforms, GameInput input)
        {
            UpdateTargetSelection(platforms);

            if (_platformTarget == -1)
                return;

            if (_failTimer > 0)
            {
                --_failTimer;
                XSpeed = _prevXDiff * XMoveSpeed;

                if (_failTimer != 0) return;
                _canFailTarget = false;
                _platformTarget = platforms.GetPlatformBelow(ref Position);
                return;
            }

            var target = platforms[_platformTarget];

            if (Position.X > target.Position.X - Platform.PlatformWidth / 2 &&
                Position.X < target.Position.X + Platform.PlatformWidth / 2)
            {
                XSpeed = 0;
                return;
            }

            var xDiff = Math.Sign(target.Position.X - Position.X);

            var failProbability = _difficulty switch
            {
                AiDifficulty.Easy => 0.45f,
                AiDifficulty.Normal => 0.25f,
                AiDifficulty.Hard => 0.05f,
                _ => 0.0f
            };

            if (Random.GetFloat() <= failProbability && _canFailTarget) _failTimer = 15;
            XSpeed = xDiff * XMoveSpeed;
            _prevXDiff = xDiff;
        }

        protected override void OnJump(Platform platform)
        {
            _selectTarget = true;
            _canFailTarget = true;
        }

        protected override void ResetSpecialFields()
        {
            _platformTarget = -1;
            _canFailTarget = true;
            _selectTarget = true;
            _failTimer = 0;
            _prevXDiff = 0;
        }

        private void UpdateTargetSelection(Platforms platforms)
        {
            if (!_selectTarget) return;

            _selectTarget = false;
            _platformTarget = platforms.GetPlatformAbove(ref Position);
        }
    }
}
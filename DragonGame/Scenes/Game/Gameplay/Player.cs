using System;
using DragonGame.Engine.Rollback;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay
{
    internal class Player : IRollbackable
    {
        public const int PlatformCollisionWidth = 16;
        public const int PlatformCollisionHeight = 16;
        private const int YTerminalSpeed = -15;
        private const int YJumpSpeed = 20;
        private const int YDrag = 1;
        private const int XMoveSpeed = 8;

        private readonly DeterministicRandom _random;

        private readonly Texture _texture;
        private bool _canFailTarget = true;
        private AiDifficulty _difficulty = AiDifficulty.Easy;

        private byte _failTimer;
        private short _platformTarget = -1;
        private int _prevXDiff;


        private bool _selectTarget = true;

        private int _xSpeed, _ySpeed;
        public bool AiControlled;
        public Point Position;

        public Player(DeterministicRandom random, Texture texture)
        {
            _random = random;
            _texture = texture;

            Position = new Point(GameField.Width / 2, 0);
        }

        public bool Descending => _ySpeed < 0;

        public void Save(StateBuffer stateBuffer)
        {
            stateBuffer.Write(Position.X);
            stateBuffer.Write(Position.Y);
            stateBuffer.Write(_xSpeed);
            stateBuffer.Write(_ySpeed);

            stateBuffer.Write(AiControlled);
            stateBuffer.Write(_selectTarget);
            stateBuffer.Write(_platformTarget);
            stateBuffer.Write(_difficulty);
            stateBuffer.Write(_failTimer);
        }

        public void Rollback(StateBuffer stateBuffer)
        {
            Position.X = stateBuffer.Read<int>();
            Position.Y = stateBuffer.Read<int>();
            _xSpeed = stateBuffer.Read<int>();
            _ySpeed = stateBuffer.Read<int>();

            AiControlled = stateBuffer.Read<bool>();
            _selectTarget = stateBuffer.Read<bool>();
            _platformTarget = stateBuffer.Read<short>();
            _difficulty = stateBuffer.Read<AiDifficulty>();
            _failTimer = stateBuffer.Read<byte>();
        }

        public void Update(Platforms platforms, GameInput input)
        {
            if (AiControlled) UpdateTargetSelection(platforms);

            UpdateX(platforms, input);
            UpdateY();
            if (Position.Y < 0) Jump();
        }


        private void UpdateTargetSelection(Platforms platforms)
        {
            if (!_selectTarget) return;

            _selectTarget = false;
            _platformTarget = platforms.GetPlatformAbove(ref Position);
        }

        public void Draw(int yScroll)
        {
            var renderer = Engine.Game.Instance.Renderer;

            var dst = new Rectangle(Position.X - _texture.Width / 2,
                GameField.TransformY(Position.Y + _texture.Height, yScroll), _texture.Width,
                _texture.Height);
            renderer.Copy(_texture, null, dst);
        }

        private void UpdateX(Platforms platforms, GameInput input)
        {
            if (AiControlled)
                UpdateXAi(platforms);
            else
                UpdateXPhysical(input);
            Position.X = Math.Min(GameField.Width, Math.Max(0, Position.X + _xSpeed));
        }

        private void UpdateXAi(Platforms platforms)
        {
            if (_platformTarget == -1)
                return;

            if (_failTimer > 0)
            {
                --_failTimer;
                _xSpeed = _prevXDiff * XMoveSpeed;

                if (_failTimer != 0) return;
                _canFailTarget = false;
                _platformTarget = platforms.GetPlatformBelow(ref Position);
                return;
            }

            var target = platforms[_platformTarget];

            if (Position.X > target.Position.X - Platform.PlatformWidth / 2 &&
                Position.X < target.Position.X + Platform.PlatformWidth / 2)
            {
                _xSpeed = 0;
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

            if (_random.GetFloat() <= failProbability && _canFailTarget) _failTimer = 15;
            _xSpeed = xDiff * XMoveSpeed;
            _prevXDiff = xDiff;
        }

        private void UpdateXPhysical(GameInput input)
        {
            if (input.HasFlag(GameInput.Left))
                _xSpeed = -XMoveSpeed;
            else if (input.HasFlag(GameInput.Right))
                _xSpeed = XMoveSpeed;
            else
                _xSpeed = 0;
        }

        private void UpdateY()
        {
            _ySpeed = Math.Max(YTerminalSpeed, _ySpeed - YDrag);
            Position.Y += _ySpeed;
        }

        public void Jump(Platform platform = null)
        {
            if (AiControlled)
            {
                _selectTarget = true;
                _canFailTarget = true;
            }

            Position.Y = platform == null ? 0 : platform.Position.Y + Platform.PlatformHeight;
            _ySpeed = YJumpSpeed;
        }
    }
}
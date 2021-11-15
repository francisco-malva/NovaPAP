using System;
using DragonGame.Engine.Rollback;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Players
{
    internal abstract class Player : IRollbackable
    {
        public const int PlatformCollisionWidth = 16;
        public const int PlatformCollisionHeight = 16;
        private const int YTerminalSpeed = -15;
        private const int YJumpSpeed = 20;
        private const int YDrag = 1;
        protected const int XMoveSpeed = 8;

        protected readonly DeterministicRandom Random;

        private readonly Texture _texture;

        protected int XSpeed, YSpeed;
        public Point Position;

        public PlayerState State { get; private set; }

        public Player(DeterministicRandom random, Texture texture)
        {
            Random = random;
            _texture = texture;

            Reset();
        }

        public void Reset()
        {
            XSpeed = 0;
            YSpeed = 0;
            Position = new Point(GameField.Width / 2, 0);
            ResetSpecialFields();
            
            SetState(PlayerState.GetReady);
        }

        protected abstract void ResetSpecialFields();

        public bool Descending => YSpeed < 0;

        public void Save(StateBuffer stateBuffer)
        {
            stateBuffer.Write(Position.X);
            stateBuffer.Write(Position.Y);
            stateBuffer.Write(XSpeed);
            stateBuffer.Write(YSpeed);

            SaveSpecialFields(stateBuffer);
        }

        protected abstract void SaveSpecialFields(StateBuffer stateBuffer);

        public void Rollback(StateBuffer stateBuffer)
        {
            Position.X = stateBuffer.Read<int>();
            Position.Y = stateBuffer.Read<int>();
            XSpeed = stateBuffer.Read<int>();
            YSpeed = stateBuffer.Read<int>();
            RollbackSpecialFields(stateBuffer);
        }

        protected abstract void RollbackSpecialFields(StateBuffer stateBuffer);

        public void Update(Platforms platforms, GameInput input)
        {
            switch (State)
            {
                case PlayerState.GetReady:
                    UpdatePosition();
                    break;
                case PlayerState.InGame:
                    MoveX(platforms, input);
                    UpdatePosition();
                    break;
                case PlayerState.Lost:
                    UpdatePosition();
                    break;
                case PlayerState.Won:
                    Position.Y += 10;
                    break;
            }
        }

        private void UpdatePosition()
        {
            if (Position.Y < 0 && State != PlayerState.Lost) Jump();
            Position.X = Math.Min(GameField.Width, Math.Max(0, Position.X + XSpeed));
            YSpeed = Math.Max(YTerminalSpeed, YSpeed - YDrag);
            Position.Y += YSpeed;
        }

        public void Draw(int yScroll)
        {
            var renderer = Engine.Game.Instance.Renderer;

            var dst = new Rectangle(Position.X - _texture.Width / 2,
                GameField.TransformY(Position.Y + _texture.Height, yScroll), _texture.Width,
                _texture.Height);
            renderer.Copy(_texture, null, dst);
        }

        protected abstract void MoveX(Platforms platforms, GameInput input);

        public void Jump(Platform platform = null)
        {
            OnJump(platform);

            Position.Y = platform == null ? 0 : platform.Position.Y + Platform.PlatformHeight;
            YSpeed = YJumpSpeed;
        }

        protected abstract void OnJump(Platform platform);
        public void SetState(PlayerState state)
        {
            State = state;
        }
    }
}
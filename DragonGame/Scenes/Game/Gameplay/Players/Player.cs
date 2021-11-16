using System;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Players
{
    internal abstract class Player
    {
        public const int PlatformCollisionWidth = 16;
        public const int PlatformCollisionHeight = 16;
        private const int YTerminalSpeed = -15;
        private const int YJumpSpeed = 20;
        private const int YDrag = 1;
        protected const int XMoveSpeed = 8;

        private readonly Texture _texture;

        protected readonly DeterministicRandom Random;
        public Point Position;

        protected int XSpeed, YSpeed;

        private double _angle = 0.0f;

        private SDL2.SDL.SDL_RendererFlip _flip;

        public Player(DeterministicRandom random, Texture texture)
        {
            Random = random;
            _texture = texture;

            Reset();
        }

        public PlayerState State { get; private set; }

        public bool Descending => YSpeed < 0;

        public void Reset()
        {
            XSpeed = 0;
            YSpeed = 0;
            Position = new Point(GameField.Width / 2, 0);
            ResetSpecialFields();

            SetState(PlayerState.GetReady);
        }

        protected abstract void ResetSpecialFields();

        public void Update(Platforms platforms, GameInput input)
        {
            UpdateMovement(platforms, input);
            UpdateAngle();
            UpdateFlip();
        }

        private void UpdateMovement(Platforms platforms, GameInput input)
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

        private void UpdateAngle()
        {
            switch (State)
            {
                case PlayerState.GetReady:
                    _angle = 0.0;
                    break;
                case PlayerState.InGame:
                    if (XSpeed > 0)
                    {
                        _angle = Math.Min(_angle + 3.0f, 25.0);
                    }
                    else if (XSpeed < 0)
                    {
                        _angle = Math.Max(_angle - 3.0f, -25.0);
                    }
                    else
                    {
                        _angle = 0;
                    }
                    break;
                case PlayerState.Won:
                    _angle = 0.0;
                    break;
                case PlayerState.Lost:
                    _angle += 15.0;
                    break;
            }
        }

        private void UpdateFlip()
        {
            switch (State)
            {
                case PlayerState.GetReady:
                    _flip = SDL2.SDL.SDL_RendererFlip.SDL_FLIP_NONE;
                    break;
                case PlayerState.InGame:
                    if (XSpeed != 0)
                    {
                        if (XSpeed < 0)
                        {
                            _flip = SDL2.SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL;
                        }
                        else
                        {
                            _flip = SDL2.SDL.SDL_RendererFlip.SDL_FLIP_NONE;
                        }
                    }

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

            renderer.CopyEx(_texture, null, dst, _angle, null, _flip);
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
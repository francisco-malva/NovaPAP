using System;
using DragonGame.Engine.Rollback;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;
using SDL2;

namespace DragonGame.Scenes.Game.Gameplay
{
    internal class GameField : IDisposable, IRollbackable
    {
        public const int Width = GameScene.Width / 2 - GameScene.GameBorder;
        public const int Height = GameScene.Height - GameScene.GameBorder;

        private readonly Texture _backgroundTexture;

        private readonly Platforms _platforms;
        private readonly Player _player;

        private int _yOffset;

        public GameField(DeterministicRandom random, Texture backgroundTexture,
            Texture playerTexture,
            Texture platformTexture)
        {
            _player = new Player(random, playerTexture);
            _platforms = new Platforms(_player, random, platformTexture);


            _backgroundTexture = backgroundTexture;

            OutputTexture = new Texture(Engine.Game.Instance.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);
        }

        public Texture OutputTexture { get; }

        public bool AiControlled
        {
            get => _player.AiControlled;
            set => _player.AiControlled = value;
        }

        public void Dispose()
        {
            OutputTexture?.Dispose();
        }

        public void Save(StateBuffer stateBuffer)
        {
            _player.Save(stateBuffer);
            _platforms.Save(stateBuffer);
        }

        public void Rollback(StateBuffer stateBuffer)
        {
            _player.Rollback(stateBuffer);
            _platforms.Rollback(stateBuffer);
        }

        public void GetReadyUpdate()
        {
            _player.Update(_platforms, GameInput.None);
        }

        public void GameUpdate(GameInput input)
        {
            _player.Update(_platforms, input);
            _platforms.Update();
            UpdateOffset();
        }

        private void UpdateOffset()
        {
            _yOffset = Math.Max(0, _player.Position.Y - Height / 2);
        }

        public void Draw()
        {
            Engine.Game.Instance.Renderer.SetTarget(OutputTexture);

            Engine.Game.Instance.Renderer.Copy(_backgroundTexture,
                new Rectangle(0, 0, _backgroundTexture.Width, _backgroundTexture.Height + _yOffset), null);
            _player.Draw(_yOffset);
            _platforms.Draw(_yOffset);
            Engine.Game.Instance.Renderer.Present();

            Engine.Game.Instance.Renderer.SetTarget(null);
        }

        public static int TransformY(int y, int yScroll)
        {
            return Height - y + yScroll;
        }
    }
}
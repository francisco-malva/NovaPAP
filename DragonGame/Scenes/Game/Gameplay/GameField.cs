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

        public readonly Platforms Platforms;
        private readonly Player _player;

        private int _yOffset;

        public GameField(DeterministicRandom random, Texture backgroundTexture,
            Texture playerTexture,
            Texture platformTexture)
        {
            _player = new Player(random, playerTexture);
            Platforms = new Platforms(_player, random, platformTexture);


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
            Platforms.Save(stateBuffer);
        }

        public void Rollback(StateBuffer stateBuffer)
        {
            _player.Rollback(stateBuffer);
            Platforms.Rollback(stateBuffer);
        }

        public void GetReadyUpdate()
        {
            _player.Update(Platforms, GameInput.None);
        }

        public void GameUpdate(GameInput input)
        {
            _player.Update(Platforms, input);
            Platforms.Update();
            UpdateOffset();
        }

        private void UpdateOffset()
        {
            _yOffset = Math.Max(0, _player.Position.Y - Height / 2);
        }

        public void Draw()
        {
            var renderer = Engine.Game.Instance.Renderer;
            renderer.SetTarget(OutputTexture);
            
            renderer.Copy(_backgroundTexture,
                new Rectangle(0, 0, _backgroundTexture.Width, _backgroundTexture.Height + _yOffset), null);
            _player.Draw(_yOffset);
            Platforms.Draw(_yOffset);

            renderer.SetTarget(null);
        }

        public static int TransformY(int y, int yScroll)
        {
            return Height - y + yScroll;
        }
    }
}
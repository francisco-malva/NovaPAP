using System;
using DragonGame.Engine.Rollback;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.AI;
using DragonGame.Scenes.Game.Gameplay.Human;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Gameplay.Players;
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
        public Player Player { get; private set; }

        private int _yOffset;

        private byte _roundsWon;
        private byte _roundsToWin;
        private byte _wonTimer;

        public bool AiControlled { get; private set; }

        public GameField(byte roundsToWin, bool ai, AiDifficulty difficulty, DeterministicRandom random, Texture backgroundTexture,
            Texture playerTexture,
            Texture platformTexture)
        {
            _roundsToWin = roundsToWin;
            AiControlled = ai;
            Player = ai ? new AIPlayer(difficulty, random, playerTexture) : new HumanPlayer(random, playerTexture);
            Platforms = new Platforms(Player, random, platformTexture);

            _backgroundTexture = backgroundTexture;

            OutputTexture = new Texture(Engine.Game.Instance.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);
        }

        public Texture OutputTexture { get; }

        public void Reset()
        {
            Platforms.Reset();
            Player.Reset();
            UpdateOffset();
        }

        public void WinRound()
        {
            Player.SetState(PlayerState.Won);
            if (_roundsWon < _roundsToWin)
            {
                _roundsWon++;
                _wonTimer = 60;
            }
        }

        public void LoseRound()
        {
            Player.SetState(PlayerState.Lost);
        }

        public bool PlayerWonGame => _roundsWon == _roundsToWin;

        public void Dispose()
        {
            OutputTexture?.Dispose();
        }

        public void Save(StateBuffer stateBuffer)
        {
            stateBuffer.Write(_roundsWon);
            stateBuffer.Write(_roundsToWin);
            stateBuffer.Write(_wonTimer);

            Player.Save(stateBuffer);
            Platforms.Save(stateBuffer);
        }

        public void Rollback(StateBuffer stateBuffer)
        {
            _roundsWon = stateBuffer.Read<byte>();
            _roundsToWin = stateBuffer.Read<byte>();
            _wonTimer = stateBuffer.Read<byte>();
            
            Player.Rollback(stateBuffer);
            Platforms.Rollback(stateBuffer);
        }

        public void Update(GameInput input)
        {
            Player.Update(Platforms, input);
            Platforms.Update();

            WinUpdate();
            if (Player.State == PlayerState.InGame)
            {
                UpdateOffset();
            }
        }

        private void WinUpdate()
        {
            if (_wonTimer > 0)
            {
                --_wonTimer;
            }
        }

        private void UpdateOffset()
        {
            _yOffset = Math.Max(0, Player.Position.Y - Height / 2);
        }

        public void Draw()
        {
            var renderer = Engine.Game.Instance.Renderer;
            renderer.SetTarget(OutputTexture);

            renderer.Copy(_backgroundTexture,
                new Rectangle(0, 0, _backgroundTexture.Width, _backgroundTexture.Height + _yOffset), null);
            Player.Draw(_yOffset);
            Platforms.Draw(_yOffset);

            renderer.SetTarget(null);
        }

        public static int TransformY(int y, int yScroll)
        {
            return Height - y + yScroll;
        }

        public bool PlayerWonRound => Platforms.PlayerFinishedCourse;
    }
}
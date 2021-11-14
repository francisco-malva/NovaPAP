using System;
using DragonGame.Engine.Rollback;
using DragonGame.Engine.Scenes;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game
{
    internal enum GameState : byte
    {
        GetReady,
        InGame,
        PlayerWon,
        GameOver
    }

    internal abstract class GameScene : Scene, IRollbackable
    {
        public const int GameBorder = 32;
        public const int Width = 640;
        public const int Height = 480;

        private readonly Texture _gameBackground;
        private readonly Texture _gameBorder;
        private readonly Texture _platformTexture;
        private readonly Texture _playerTexture;

        protected readonly GameField P1Field;
        protected readonly GameField P2Field;

        protected readonly DeterministicRandom Random;

        private byte _roundsToWin;

        private GameState _state = GameState.GetReady;
        private byte _stateTimer;

        protected GameScene(byte roundsToWin)
        {
            FrameCount = 0;

            _roundsToWin = roundsToWin;

            _gameBorder = Texture.FromBmp("Assets/Textures/UI/game-border.bmp", Engine.Game.Instance.Renderer);
            _gameBackground = Texture.FromBmp("Assets/Textures/Game/background.bmp", Engine.Game.Instance.Renderer);
            _playerTexture = Texture.FromBmp("Assets/Textures/Game/player.bmp", Engine.Game.Instance.Renderer);
            _platformTexture = Texture.FromBmp("Assets/Textures/Game/platform.bmp", Engine.Game.Instance.Renderer);
            Random = new DeterministicRandom();

            P1Field = new GameField(Random, _gameBackground, _playerTexture, _platformTexture);
            P2Field = new GameField(Random, _gameBackground, _playerTexture, _platformTexture);
        }

        protected ulong FrameCount { get; private set; }

        public void Save(StateBuffer stateBuffer)
        {
            stateBuffer.Write(_roundsToWin);
            stateBuffer.Write(FrameCount);
            
            stateBuffer.Write(_state);
            stateBuffer.Write(_stateTimer);

            Random.Save(stateBuffer);
            P1Field.Save(stateBuffer);
            P2Field.Save(stateBuffer);
        }

        public void Rollback(StateBuffer stateBuffer)
        {
            _roundsToWin = stateBuffer.Read<byte>();
            FrameCount = stateBuffer.Read<ulong>();

            _state = stateBuffer.Read<GameState>();
            _stateTimer = stateBuffer.Read<byte>();

            Random.Rollback(stateBuffer);
            P1Field.Rollback(stateBuffer);
            P2Field.Rollback(stateBuffer);
        }

        public override void OnTick()
        {
            RunFrame();
        }

        protected virtual void RunFrame()
        {
            ++FrameCount;
        }

        protected void Update(GameInput p1Input, GameInput p2Input)
        {
            switch (_state)
            {
                case GameState.GetReady:
                    P1Field.Platforms.CanCollide = true;
                    P2Field.Platforms.CanCollide = true;
                    _state = GameState.InGame;
                    break;
                case GameState.InGame:
                    break;
                case GameState.PlayerWon:
                    break;
                case GameState.GameOver:
                    break;
                default:
                    break;
            }
            P1Field.GameUpdate(p1Input);
            P2Field.GameUpdate(p2Input);
        }

        protected void Draw()
        {
            var renderer = Engine.Game.Instance.Renderer;

            P1Field.Draw();
            P2Field.Draw();

            var p1Dest = new Rectangle(GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);
            var p2Dest = new Rectangle(Width / 2 + GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);


            renderer.SetDrawColor(0, 0, 0, 0);
            renderer.Clear();
            renderer.Copy(_gameBorder, null, null);
            renderer.Copy(P1Field.OutputTexture, null, p1Dest);
            renderer.Copy(P2Field.OutputTexture, null, p2Dest);
            renderer.Present();
        }

        protected override void OnUnload()
        {
            P1Field.Dispose();
            P2Field.Dispose();

            _gameBorder.Dispose();
            _gameBackground.Dispose();
            _playerTexture.Dispose();
            _platformTexture.Dispose();
        }
    }
}
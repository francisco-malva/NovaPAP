using DragonGame.Engine.Rollback;
using DragonGame.Engine.Scenes;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game
{
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
        private Winner _winner;

        protected GameScene(byte roundsToWin, bool p1Ai = false, bool p2Ai = false, AiDifficulty difficulty = AiDifficulty.Easy)
        {
            FrameCount = 0;

            _roundsToWin = roundsToWin;

            _gameBorder = Texture.FromBmp("Assets/Textures/UI/game-border.bmp", Engine.Game.Instance.Renderer);
            _gameBackground = Texture.FromBmp("Assets/Textures/Game/background.bmp", Engine.Game.Instance.Renderer);
            _playerTexture = Texture.FromBmp("Assets/Textures/Game/player.bmp", Engine.Game.Instance.Renderer);
            _platformTexture = Texture.FromBmp("Assets/Textures/Game/platform.bmp", Engine.Game.Instance.Renderer);
            Random = new DeterministicRandom();

            P1Field = new GameField(roundsToWin, p1Ai, difficulty, Random, _gameBackground, _playerTexture, _platformTexture);
            P2Field = new GameField(roundsToWin, p2Ai, difficulty, Random, _gameBackground, _playerTexture, _platformTexture);
        }

        protected ulong FrameCount { get; private set; }

        private void EndRound(Winner winner)
        {
            _winner = winner;
            ChangeState(GameState.PlayerWon);
        }

        public void Save(StateBuffer stateBuffer)
        {
            stateBuffer.Write(_roundsToWin);
            stateBuffer.Write(FrameCount);

            stateBuffer.Write(_state);
            stateBuffer.Write(_stateTimer);

            stateBuffer.Write(_winner);

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

            _winner = stateBuffer.Read<Winner>();

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

        private void ChangeState(GameState state)
        {
            _stateTimer = 0;
            _state = state;

            switch (_state)
            {
                case GameState.GetReady:
                    P1Field.Reset();
                    P2Field.Reset();
                    break;
                case GameState.InGame:
                    P1Field.Player.SetState(PlayerState.InGame);
                    P2Field.Player.SetState(PlayerState.InGame);
                    break;
                case GameState.PlayerWon:
                    switch (_winner)
                    {
                        case Winner.Both:
                            P1Field.WinRound();
                            P2Field.WinRound();
                            break;
                        case Winner.P1:
                            P1Field.WinRound();
                            P2Field.LoseRound();
                            break;
                        case Winner.P2:
                            P2Field.WinRound();
                            P1Field.LoseRound();
                            break;
                    }

                    break;
                case GameState.GameOver:
                    break;
                default:
                    break;
            }
        }

        protected void Update(GameInput p1Input, GameInput p2Input)
        {
            ++_stateTimer;
            switch (_state)
            {
                case GameState.GetReady:
                    if (_stateTimer == 120)
                    {
                        ChangeState(GameState.InGame);
                        return;
                    }
                    P1Field.Update(p1Input);
                    P2Field.Update(p2Input);
                    break;
                case GameState.InGame:
                    P1Field.Update(p1Input);
                    P2Field.Update(p2Input);

                    if (P1Field.PlayerWonRound && P2Field.PlayerWonRound)
                    {
                        EndRound(Winner.Both);
                    }
                    else if (P1Field.PlayerWonRound)
                    {
                        EndRound(Winner.P1);
                    }
                    else if (P2Field.PlayerWonRound)
                    {
                        EndRound(Winner.P2);
                    }
                    break;
                case GameState.PlayerWon:
                    P1Field.Update(p1Input);
                    P2Field.Update(p2Input);

                    if (_stateTimer == 60)
                    {
                        if (P1Field.PlayerWonGame || P2Field.PlayerWonGame)
                        {
                            Engine.Game.Instance.SceneManager.Set(new MainMenu.MainMenuScene());
                        }
                        ChangeState(GameState.GetReady);
                    }
                    break;
                case GameState.GameOver:
                    break;
                default:
                    break;
            }
        }

        public void QueryAi(out bool p1Ai, out bool p2Ai)
        {
            p1Ai = P1Field.AiControlled;
            p2Ai = P2Field.AiControlled;
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
            OnGameEnd();
            P1Field.Dispose();
            P2Field.Dispose();

            _gameBorder.Dispose();
            _gameBackground.Dispose();
            _playerTexture.Dispose();
            _platformTexture.Dispose();
        }

        protected abstract void OnGameEnd();
    }
}
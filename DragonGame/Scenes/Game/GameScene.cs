using System;
using DragonGame.Engine.Input;
using DragonGame.Engine.Scenes;
using DragonGame.Engine.Utilities;
using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using DragonGame.Scenes.Game.Input;
using DragonGame.Scenes.MainMenu;
using Engine.Wrappers.SDL2;
using SDL2;

namespace DragonGame.Scenes.Game
{
    internal abstract class GameScene : Scene
    {
        public const int GameBorder = 32;
        public const int Width = 640;
        public const int Height = 480;

        public const byte GetReadyTime = 120;
        public const byte RoundEndTime = 120;

        private readonly Texture _gameBackground;
        private readonly Texture _gameBorder;
        private readonly Texture _platformTexture;
        private readonly Texture _playerTexture;
        private readonly Texture _checkmarkTexture;
        private readonly Texture _getReadyTexture;
        private readonly Texture _goTexture;
        private readonly Texture _winnerTexture;
        private readonly Texture _youLoseTexture;
        private readonly Texture _drawTexture;

        protected readonly GameField P1Field;
        protected readonly GameField P2Field;

        protected readonly DeterministicRandom Random;

        private GameState _state = GameState.GetReady;
        private byte _stateTimer;
        private Winner _winner;

        protected GameScene(byte roundsToWin, bool p1Ai = false, bool p2Ai = false,
            AiDifficulty difficulty = AiDifficulty.Easy)
        {
            FrameCount = 0;

            _gameBorder = Texture.FromBmp("UI/game-border");
            _gameBackground = Texture.FromBmp("Game/background");
            _playerTexture = Texture.FromBmp("Game/player");
            _platformTexture = Texture.FromBmp("Game/platform");
            _platformTexture.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            _checkmarkTexture = Texture.FromBmp("Game/checkmark");
            _getReadyTexture = Texture.FromBmp("Game/Banners/get-ready");
            _goTexture = Texture.FromBmp("Game/Banners/go");
            _winnerTexture = Texture.FromBmp("Game/Banners/winner");
            _youLoseTexture = Texture.FromBmp("Game/Banners/you-lose");
            _drawTexture = Texture.FromBmp("Game/Banners/draw");
            Random = new DeterministicRandom();

            P1Field = new GameField(roundsToWin, p1Ai, difficulty, Random, _gameBackground, _playerTexture,
                _platformTexture, _checkmarkTexture, _getReadyTexture, _goTexture, _winnerTexture, _youLoseTexture,
                _drawTexture);
            P2Field = new GameField(roundsToWin, p2Ai, difficulty, Random, _gameBackground, _playerTexture,
                _platformTexture, _checkmarkTexture, _getReadyTexture, _goTexture, _winnerTexture, _youLoseTexture,
                _drawTexture);
        }

        protected void SetRoundsToWin(byte roundsToWin)
        {
            P1Field.RoundsToWin = roundsToWin;
            P2Field.RoundsToWin = roundsToWin;
        }

        protected ulong FrameCount { get; private set; }

        private void EndRound(Winner winner)
        {
            _winner = winner;
            ChangeState(GameState.PlayerWon);
        }

        protected void SimulateAndDraw(GameInput p1Input, GameInput p2Input)
        {
            SimulateFrame(p1Input, p2Input);
            Draw();
            ++FrameCount;
        }

        protected static void ProcessInput(ref GameInput input,
            SDL.SDL_Scancode leftScanCode,
            SDL.SDL_Scancode rightScanCode, SDL.SDL_Scancode specialScanCode)
        {
            input = GameInput.None;

            input |= Keyboard.KeyDown(leftScanCode) ? GameInput.Left : GameInput.None;
            input |= Keyboard.KeyDown(rightScanCode) ? GameInput.Right : GameInput.None;
            input |= Keyboard.KeyDown(specialScanCode) ? GameInput.Special : GameInput.None;
        }

        protected void ChangeState(GameState state)
        {
            _stateTimer = 0;
            _state = state;

            switch (_state)
            {
                case GameState.GetReady:
                    P1Field.GetReady();
                    P2Field.GetReady();
                    break;
                case GameState.InGame:
                    P1Field.BeginRound();
                    P2Field.BeginRound();
                    break;
                case GameState.PlayerWon:
                    switch (_winner)
                    {
                        case Winner.Both:
                            P1Field.WinRound(true);
                            P2Field.WinRound(true);
                            break;
                        case Winner.P1:
                            P1Field.WinRound();
                            P2Field.LoseRound();
                            break;
                        case Winner.P2:
                            P2Field.WinRound();
                            P1Field.LoseRound();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case GameState.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SimulateFrame(GameInput p1Input, GameInput p2Input)
        {
            ++_stateTimer;
            switch (_state)
            {
                case GameState.GetReady:
                    if (_stateTimer == GetReadyTime)
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
                        EndRound(Winner.Both);
                    else if (P1Field.PlayerWonRound)
                        EndRound(Winner.P1);
                    else if (P2Field.PlayerWonRound) EndRound(Winner.P2);
                    break;
                case GameState.PlayerWon:
                    P1Field.Update(p1Input);
                    P2Field.Update(p2Input);

                    if (_stateTimer == RoundEndTime)
                    {
                        if (P1Field.PlayerWonGame || P2Field.PlayerWonGame)
                            Engine.Game.Instance.SceneManager.Set(new MainMenuScene());
                        ChangeState(GameState.GetReady);
                    }

                    break;
                case GameState.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void QueryAi(out bool p1Ai, out bool p2Ai)
        {
            p1Ai = P1Field.AiControlled;
            p2Ai = P2Field.AiControlled;
        }

        private void Draw()
        {
            var renderer = Engine.Game.Instance.Renderer;

            P1Field.Draw();
            P2Field.Draw();

            var p1Dest = new Rectangle(GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);
            var p2Dest = new Rectangle(Width / 2 + GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);

            renderer.SetDrawColor(Color.Black);
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
            _checkmarkTexture.Dispose();
            
            _getReadyTexture.Dispose();
            _goTexture.Dispose();
            _winnerTexture.Dispose();
            _youLoseTexture.Dispose();
            _drawTexture.Dispose();
        }

        protected abstract void OnGameEnd();
    }
}
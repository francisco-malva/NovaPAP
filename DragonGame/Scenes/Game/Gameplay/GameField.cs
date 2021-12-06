using System;
using DragonGame.Engine.Utilities;
using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using DragonGame.Scenes.Game.Input;
using DuckDuckJump.Scenes.Game.Gameplay;
using DuckDuckJump.Scenes.Game.Gameplay.Score;
using Engine.Wrappers.SDL2;
using SDL2;

namespace DragonGame.Scenes.Game.Gameplay
{
    internal class GameField : IDisposable
    {
        public const int Width = GameScene.Width / 2 - GameScene.GameBorder;
        public const int Height = GameScene.Height - GameScene.GameBorder;

        private readonly Texture _backgroundTexture;

        public readonly Platforms Platforms;

        private BannerDisplay _bannerDisplay;
        public Scoreboard Scoreboard;
        private FinishLine _finishLine;
        private Camera _camera;

        public GameField(byte roundsToWin, bool ai, AiDifficulty difficulty, DeterministicRandom random)
        {
            AiControlled = ai;
            Player = ai ? new AIPlayer(difficulty, random) : new HumanPlayer(random);

            Scoreboard = new Scoreboard(Player, roundsToWin);
            Platforms = new Platforms(Player, random);

            _backgroundTexture = Engine.Game.Instance.TextureManager["Game/background"];
            _backgroundTexture.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            _bannerDisplay = new BannerDisplay();
            _finishLine = new FinishLine(Player, Platforms.FinishingY);

            _camera = new Camera(new Point(Width, Height), new Rectangle(0, 0, GameField.Width, Platforms.FinishingY));

            OutputTexture = new Texture(Engine.Game.Instance.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);
        }

        public Player Player { get; }

        public bool AiControlled { get; }

        public Texture OutputTexture { get; }

        public bool PlayerWonGame => Scoreboard.WonGame;

        public bool PlayerWonRound => _finishLine.CrossedFinishLine;

        public void Dispose()
        {
            OutputTexture?.Dispose();
        }

        public void GetReady()
        {
            _finishLine.Decreasing = false;
            _bannerDisplay.Raise(BannerType.GetReady, GameScene.GetReadyTime);
            Platforms.GeneratePlatforms();
            Player.GetReady();
            UpdateCamera();
        }

        public void BeginRound()
        {
            _finishLine.Decreasing = false;
            _bannerDisplay.Raise(BannerType.Go, GameScene.GetReadyTime / 4);
            Player.SetState(PlayerState.InGame);
        }

        public void WinRound(bool draw = false)
        {
            _finishLine.Decreasing = true;
            Player.SetState(PlayerState.Won);

            _bannerDisplay.Raise(draw ? BannerType.Draw : BannerType.Winner, GameScene.RoundEndTime);
            Scoreboard.WinRound(draw, GameScene.RoundEndTime);
        }

        public void LoseRound()
        {
            _finishLine.Decreasing = true;
            _bannerDisplay.Raise(BannerType.YouLose, GameScene.RoundEndTime);
            Player.SetState(PlayerState.Lost);
        }

        public void Update(GameInput input)
        {
            Player.Update(Platforms, input);
            Platforms.Update();

            _bannerDisplay.Update();
            _finishLine.Update();
            UpdateCamera();

            switch (Player.State)
            {
                case PlayerState.InGame:
                    break;
                case PlayerState.Won:
                    Scoreboard.Update();
                    break;
                case PlayerState.GetReady:
                    break;
                case PlayerState.Lost:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input));
            }
        }

        private void UpdateCamera() => _camera.Position = new Point(0, Player.Position.Y - Height / 2);

        public void Draw()
        {
            var renderer = Engine.Game.Instance.Renderer;
            renderer.SetTarget(OutputTexture);

            DrawBackground();
            DrawGameElements();
            Scoreboard.Draw();
            _bannerDisplay.Draw();

            renderer.SetTarget(null);
        }

        private void DrawGameElements()
        {
            Player.Draw(_camera);
            Platforms.Draw(_camera);
            _finishLine.Draw(_camera);
        }

        private void DrawBackground()
        {
            var renderer = Engine.Game.Instance.Renderer;

            var nightExposure = Mathematics.Lerp(0.0f, 255.0f,
                Platforms.GetClimbingProgress(_camera.Position.Y == 0 ? 0 : Player.Position.Y));

            //Drawing the day
            _backgroundTexture.SetAlphaMod(255);
            renderer.Copy(_backgroundTexture,
                new Rectangle(0, 0, 250, 250),
                new Rectangle(0, 0, 250 * 2, 250 * 2));

            _backgroundTexture.SetAlphaMod((byte)nightExposure);
            renderer.Copy(_backgroundTexture,
                new Rectangle(250, 0, 250 * 2, 250 * 2),
                new Rectangle(0, 0, 250 * 2, 250 * 2));
        }

        public static int TransformY(int y, int yScroll)
        {
            return Height - y + yScroll;
        }
    }
}
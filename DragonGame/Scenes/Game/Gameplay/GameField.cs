using System;
using DragonGame.Engine.Utilities;
using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using DragonGame.Scenes.Game.Input;
using Engine.Wrappers.SDL2;
using SDL2;

namespace DragonGame.Scenes.Game.Gameplay
{
    internal class GameField : IDisposable
    {
        public const int Width = GameScene.Width / 2 - GameScene.GameBorder;
        public const int Height = GameScene.Height - GameScene.GameBorder;

        private readonly Texture _backgroundTexture;
        private readonly Texture _checkmarkTexture;
        private readonly Texture _drawTexture;

        private readonly Texture _getReadyTexture;
        private readonly Texture _goTexture;
        private readonly Texture _winnerTexture;
        private readonly Texture _youLoseTexture;

        public readonly Platforms Platforms;
        private ushort _bannerDuration;

        private Texture _bannerTexture;
        private ushort _bannerTimer;
        private ushort _blinkTimer;

        /// <summary>
        ///     Is the checkmark that indicates the won round dark?
        /// </summary>
        private bool _checkmarkDark;

        private byte _roundsWon;

        /// <summary>
        ///     By how much to affect the drawing operations in the Y axis for the game elements (player, platforms, etc)
        /// </summary>
        private int _yScroll;

        public byte RoundsToWin;

        public GameField(byte roundsToWin, bool ai, AiDifficulty difficulty, DeterministicRandom random)
        {
            RoundsToWin = roundsToWin;
            AiControlled = ai;
            Player = ai ? new AIPlayer(difficulty, random) : new HumanPlayer(random);
            Platforms = new Platforms(Player, random);

            _backgroundTexture = Engine.Game.Instance.TextureManager["Game/background"];
            _backgroundTexture.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            _checkmarkTexture = Engine.Game.Instance.TextureManager["Game/checkmark"];
            _getReadyTexture = Engine.Game.Instance.TextureManager["Game/Banners/get-ready"];
            _goTexture = Engine.Game.Instance.TextureManager["Game/Banners/go"];
            _winnerTexture = Engine.Game.Instance.TextureManager["Game/Banners/winner"];
            _youLoseTexture = Engine.Game.Instance.TextureManager["Game/Banners/you_lose"];
            _drawTexture = Engine.Game.Instance.TextureManager["Game/Banners/draw"];

            OutputTexture = new Texture(Engine.Game.Instance.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);
        }

        public Player Player { get; }

        public bool AiControlled { get; }

        public Texture OutputTexture { get; }

        public bool PlayerWonGame => _roundsWon == RoundsToWin;

        public bool PlayerWonRound => Platforms.PlayerFinishedCourse;

        public void Dispose()
        {
            OutputTexture?.Dispose();
        }

        public void GetReady()
        {
            RaiseBanner(Banner.GetReady, GameScene.GetReadyTime);
            Platforms.GeneratePlatforms();
            Player.GetReady();
            UpdateOffset();
        }

        public void BeginRound()
        {
            RaiseBanner(Banner.Go, GameScene.GetReadyTime / 4);
            Player.SetState(PlayerState.InGame);
        }

        public void WinRound(bool draw = false)
        {
            Player.SetState(PlayerState.Won);

            RaiseBanner(draw ? Banner.Draw : Banner.Winner, GameScene.RoundEndTime);

            if (draw && _roundsWon == RoundsToWin - 1)
                return;
            _roundsWon++;
            _blinkTimer = GameScene.RoundEndTime;
        }

        public void LoseRound()
        {
            RaiseBanner(Banner.YouLose, GameScene.RoundEndTime);
            Player.SetState(PlayerState.Lost);
        }

        public void Update(GameInput input)
        {
            Player.Update(Platforms, input);
            Platforms.Update();

            switch (Player.State)
            {
                case PlayerState.InGame:
                    UpdateOffset();
                    break;
                case PlayerState.Won:
                    CheckmarkBlinkUpdate();
                    break;
                case PlayerState.GetReady:
                    break;
                case PlayerState.Lost:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input));
            }
        }

        private Texture GetTextureForBanner(Banner bannerType)
        {
            return bannerType switch
            {
                Banner.GetReady => _getReadyTexture,
                Banner.Go => _goTexture,
                Banner.Winner => _winnerTexture,
                Banner.YouLose => _youLoseTexture,
                Banner.Draw => _drawTexture,
                _ => throw new ArgumentOutOfRangeException(nameof(bannerType), bannerType, null)
            };
        }

        public void RaiseBanner(Banner bannerType, ushort duration)
        {
            _bannerTexture = GetTextureForBanner(bannerType);
            _bannerTimer = duration;
            _bannerDuration = duration;
        }

        private void CheckmarkBlinkUpdate()
        {
            if (_blinkTimer > 0)
            {
                --_blinkTimer;

                if (_blinkTimer % 25 == 0) _checkmarkDark = !_checkmarkDark;
            }
            else
            {
                _checkmarkDark = false;
            }
        }

        private void UpdateOffset()
        {
            _yScroll = Math.Max(0, Player.Position.Y - Height / 2);
        }

        public void Draw()
        {
            var renderer = Engine.Game.Instance.Renderer;
            renderer.SetTarget(OutputTexture);

            DrawBackground();
            DrawGameElements();
            DrawScoreboard();
            DrawBanner();

            renderer.SetTarget(null);
        }

        private void DrawBanner()
        {
            if (_bannerTexture == null) return;

            if (_bannerTimer == 0)
            {
                _bannerTexture = null;
            }
            else
            {
                _bannerTexture.SetAlphaMod((byte)(_bannerTimer / (float)_bannerDuration * 255.0f));
                Engine.Game.Instance.Renderer.Copy(_bannerTexture, null,
                    new Rectangle(Width / 2 - _bannerTexture.Width / 2,
                        Height / 2 - _bannerTexture.Height / 2, _bannerTexture.Width, _bannerTexture.Height));
                --_bannerTimer;
            }
        }

        private void DrawGameElements()
        {
            Player.Draw(_yScroll);
            Platforms.Draw(_yScroll);
        }

        private void DrawBackground()
        {
            var renderer = Engine.Game.Instance.Renderer;

            var nightExposure = Mathematics.Lerp(0.0f, 255.0f,
                Platforms.GetClimbingProgress(_yScroll == 0 ? 0 : Player.Position.Y));

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

        private void DrawScoreboard()
        {
            var renderer = Engine.Game.Instance.Renderer;

            for (var i = 0; i < RoundsToWin; i++)
            {
                if (i > _roundsWon - 1)
                    _checkmarkTexture.SetColorMod(Color.Black);
                else if (i == _roundsWon - 1 && Player.State == PlayerState.Won)
                    _checkmarkTexture.SetColorMod(_checkmarkDark ? Color.Black : Color.White);
                else
                    _checkmarkTexture.SetColorMod(Color.White);
                renderer.Copy(_checkmarkTexture, null, new Rectangle(8 + 20 * i, 8, 16, 16));
            }
        }

        public static int TransformY(int y, int yScroll)
        {
            return Height - y + yScroll;
        }
    }
}
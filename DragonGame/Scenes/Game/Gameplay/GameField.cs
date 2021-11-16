using System;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;
using SDL2;

namespace DragonGame.Scenes.Game.Gameplay
{
    internal class GameField : IDisposable
    {
        public const int Width = GameScene.Width / 2 - GameScene.GameBorder;
        public const int Height = GameScene.Height - GameScene.GameBorder;

        private readonly Texture _backgroundTexture;
        private readonly Texture _checkmarkTexture;

        private readonly Texture _getReadyTexture;
        private readonly Texture _goTexture;
        private readonly Texture _winnerTexture;
        private readonly Texture _youLoseTexture;
        private readonly Texture _drawTexture;

        private readonly byte _roundsToWin;

        public readonly Platforms Platforms;

        private byte _roundsWon;
        private ushort _wonTimer;


        private Texture _bannerTexture;
        private ushort _bannerTimer = 0;
        private ushort _bannerDuration = 0;

        /// <summary>
        /// By how much to affect the drawing operations in the Y axis for the game elements (player, platforms, etc)
        /// </summary>
        private int _yScroll;

        /// <summary>
        /// Is the checkmark that indicates the won round dark?
        /// </summary>
        private bool _checkmarkDark;

        public GameField(byte roundsToWin, bool ai, AiDifficulty difficulty, DeterministicRandom random,
            Texture backgroundTexture,
            Texture playerTexture,
            Texture platformTexture, Texture checkmarkTexture, Texture getReadyTexture, Texture goTexture,
            Texture winnerTexture, Texture youLoseTexture, Texture drawTexture)
        {
            _roundsToWin = roundsToWin;
            AiControlled = ai;
            Player = ai ? new AIPlayer(difficulty, random, playerTexture) : new HumanPlayer(random, playerTexture);
            Platforms = new Platforms(Player, random, platformTexture);

            _backgroundTexture = backgroundTexture;
            _checkmarkTexture = checkmarkTexture;
            _getReadyTexture = getReadyTexture;
            _goTexture = goTexture;
            _winnerTexture = winnerTexture;
            _youLoseTexture = youLoseTexture;
            _drawTexture = drawTexture;

            OutputTexture = new Texture(Engine.Game.Instance.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);
        }

        public Player Player { get; }

        public bool AiControlled { get; }

        public Texture OutputTexture { get; }

        public bool PlayerWonGame => _roundsWon == _roundsToWin;

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

            if(draw && _roundsWon == _roundsToWin - 1)
                return;
            _roundsWon++;
            _wonTimer = GameScene.RoundEndTime;
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
                    WinUpdate();
                    break;
                case PlayerState.GetReady:
                    break;
                case PlayerState.Lost:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        private void WinUpdate()
        {
            if (_wonTimer > 0)
            {
                --_wonTimer;

                if (_wonTimer % 25 == 0)
                {
                    _checkmarkDark = !_checkmarkDark;
                }
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
            if (_bannerTexture == null)
            {
                return;
            }

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
            renderer.Copy(_backgroundTexture,
                new Rectangle(0, 0, _backgroundTexture.Width, _backgroundTexture.Height + _yScroll), new Rectangle(0,0,_backgroundTexture.Width * 2,_backgroundTexture.Height * 2));
        }

        private void DrawScoreboard()
        {
            var renderer = Engine.Game.Instance.Renderer;

            for (var i = 0; i < _roundsToWin; i++)
            {
                if (i > _roundsWon - 1)
                {
                    _checkmarkTexture.SetColorMod(0, 0, 0);
                }
                else if(i == _roundsWon - 1 && Player.State == PlayerState.Won)
                {
                    var colorMod = _checkmarkDark ? (byte)0 : (byte)255;
                    _checkmarkTexture.SetColorMod(colorMod, colorMod, colorMod);
                }
                else
                {
                    _checkmarkTexture.SetColorMod(255, 255, 255);
                }
                renderer.Copy(_checkmarkTexture, null, new Rectangle(8 + 20 * i, 8, 16, 16));
            }
        }

        public static int TransformY(int y, int yScroll)
        {
            return Height - y + yScroll;
        }
    }
}
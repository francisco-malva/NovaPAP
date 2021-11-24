using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Players;
using Engine.Wrappers.SDL2;

namespace DuckDuckJump.Scenes.Game.Gameplay.Score
{
    internal sealed class Scoreboard
    {
        private Texture _checkmark;
        public byte RoundsToWin;
        private byte _roundsWon;

        private ushort _blinkTimer;

        /// <summary>
        ///     Is the checkmark that indicates the won round dark?
        /// </summary>
        private bool _checkmarkDark;

        private Player _player;

        public Scoreboard(Player player, byte roundsToWin)
        {
            _checkmark = DragonGame.Engine.Game.Instance.TextureManager["Game/checkmark"];
            _player = player;
            RoundsToWin = roundsToWin;
            _roundsWon = 0;
        }

        public void WinRound(bool draw, ushort blinkTime)
        {
            if (draw && _roundsWon == RoundsToWin - 1)
                return;
            _roundsWon++;
            _blinkTimer = blinkTime;
        }

        public void Update()
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

        public void Draw()
        {
            var renderer = DragonGame.Engine.Game.Instance.Renderer;

            for (var i = 0; i < RoundsToWin; i++)
            {
                if (i > _roundsWon - 1)
                    _checkmark.SetColorMod(Color.Black);
                else if (i == _roundsWon - 1 && _player.State == PlayerState.Won)
                    _checkmark.SetColorMod(_checkmarkDark ? Color.Black : Color.White);
                else
                    _checkmark.SetColorMod(Color.White);
                renderer.Copy(_checkmark, null, new Rectangle(8 + 20 * i, 8, 16, 16));
            }
        }
        public bool WonGame => RoundsToWin == _roundsWon;
    }
}
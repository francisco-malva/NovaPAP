using System;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Gameplay
{
    internal class FinishLine
    {
        private byte _alpha;

        private readonly Texture _finishLine;

        private readonly Player _player;
        private readonly int _y;
        public bool Decreasing;

        public FinishLine(Player player, int y)
        {
            _player = player;
            _y = y;

            _finishLine = Engine.Game.Instance.TextureManager["Game/finish-line"];
            _finishLine.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
        }

        public bool CrossedFinishLine => _player.Position.Y + Player.PlatformCollisionHeight >= _y;

        public void Update()
        {
            if (Decreasing)
            {
                if (_alpha > 0)
                {
                    var alpha = (short) Math.Max(_alpha - 5, 0);
                    _alpha = (byte) alpha;
                }
            }
            else
            {
                if (_alpha < byte.MaxValue)
                {
                    var alpha = (short) Math.Min(_alpha + 5, byte.MaxValue);
                    _alpha = (byte) alpha;
                }
            }
        }

        public void Draw(Camera _camera)
        {
            var screenPosition = _camera.TransformPoint(new Point(0, _y - _finishLine.Height / 2));
            var dst = new Rectangle(screenPosition.X, screenPosition.Y, _finishLine.Width, _finishLine.Height);
            if (!_camera.OnScreen(dst)) return;

            _finishLine.SetAlphaMod(_alpha);
            Engine.Game.Instance.Renderer.Copy(_finishLine, null, dst);
        }
    }
}
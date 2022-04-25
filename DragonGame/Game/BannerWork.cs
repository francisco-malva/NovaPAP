#region

using System;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Utilities;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    private static class BannerWork
    {
        private const float EndTime = 0.25f;
        private static string _message;
        private static Color _color;
        private static float _time;
        private static Size _size;

        private static float _scale;

        public static void SetMessage(string message, Color color, float time)
        {
            _message = message;
            _color = color;
            _time = time;
            _size = Assets.Font(Assets.FontIndex.BannerFont).MeasureString(message);
        }

        public static void Reset()
        {
            _time = -EndTime;
        }

        public static void Update()
        {
            _time -= GameFlow.TimeStep;

            if (_time < -EndTime) _time = -EndTime;

            _scale = _time >= 0.0f ? 1.0f : Mathematics.Map(MathF.Abs(_time), 0.0f, EndTime, 1.0f, 0.0f);
        }

        public static void DrawMe()
        {
            if (!_info.FightMessages || IsDone() || _message == null)
                return;

            Assets.Font(Assets.FontIndex.BannerFont).Draw(_message,
                Matrix3x2.CreateScale(1.0f, _scale) * Matrix3x2.CreateTranslation(
                    Graphics.Midpoint.X - _size.Width / 2.0f,
                    Graphics.Midpoint.Y - _size.Height / 2.0f), _color);
        }

        public static bool IsDone()
        {
            return Math.Abs(_time + EndTime) < float.Epsilon;
        }
    }
}
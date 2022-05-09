#region

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Game.Configuration;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    internal static class BannerWork
    {
        public enum MessageIndex : byte
        {
            Round,
            Wins,
            Draw,
            GameSet,
            TimeUp,
            TimeAttackStart,
            WatchModeStart,
            NoBanner = byte.MaxValue
        }

        private const float EndTime = 0.25f;

        private static readonly BannerMessage[] Messages =
        {
            new("ROUND %r", Color.White, 1.0f),
            new("%w WINS!", Color.DarkGoldenrod, 1.0f),
            new("DRAW!", Color.DodgerBlue, 1.0f),
            new("GAME SET!", Color.DarkRed, 2.0f),
            new("TIME'S UP!", Color.Chocolate, 1.0f),
            new("TIME ATTACK!", Color.DarkGoldenrod, 1.0f),
            new("WATCH MODE!", Color.DarkGoldenrod, 1.0f)
        };

        private static MessageIndex _currentMessage;
        private static string _message;
        private static Color _color;
        private static float _time;
        private static Size _size;

        private static float _scale;

        public static void SaveMe(Stream stream)
        {
            stream.Write(_currentMessage);
            stream.Write(_time);
            stream.Write(_scale);
        }

        public static void LoadMe(Stream stream)
        {
            SetMessage(stream.Read<MessageIndex>());
            _time = stream.Read<float>();
            _scale = stream.Read<float>();
        }

        public static void SetMessage(MessageIndex index)
        {
            _currentMessage = index;
            _message = Messages[(int) index].Message.Replace("%r", _currentRound.ToString())
                .Replace("%w", _winner.ToString()).Replace("%n", Settings.MyData.Nickname.ToString());
            _color = Messages[(int) index].Color;
            _time = Messages[(int) index].Time;
            _size = Assets.Font(Assets.FontIndex.BannerFont).MeasureString(_message);
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
            if (State != MatchState.BeginningMessage && ((_info.GameFlags & GameInfo.Flags.Exhibition) != 0 || IsDone() || _message == null))
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

        private struct BannerMessage
        {
            public readonly string Message;
            public readonly Color Color;
            public readonly float Time;

            public BannerMessage(string message, Color color, float time)
            {
                Message = message;
                Color = color;
                Time = time;
            }
        }
    }
}
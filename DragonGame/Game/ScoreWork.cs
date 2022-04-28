#region

using System.Drawing;
using System.IO;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Utilities;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    private static class ScoreWork
    {
        private static readonly sbyte[] Scores = new sbyte[PlayerCount];

        public static void Save(Stream stream)
        {
            for (var i = 0; i < PlayerCount; i++) stream.Write(Scores[i]);
        }

        public static void Load(Stream stream)
        {
            for (var i = 0; i < PlayerCount; i++) Scores[i] = stream.Read<sbyte>();
        }

        public static void Reset()
        {
            for (var i = 0; i < Scores.Length; i++) Scores[i] = 0;
        }

        public static void IncreaseScore(int playerIndex)
        {
            ++Scores[playerIndex];
        }

        public static void DrawMe()
        {
            if (_info.ScoreCount <= 0) return;

            for (var i = 0; i < PlayerCount; i++)
            for (var j = 0; j < _info.ScoreCount; j++)
            {
                var isFirstPlayer = (i + 1) % 2 != 0;

                var texture = Assets.Texture(Assets.TextureIndex.ScoreIcon);

                var query = texture.QueryTexture();

                var stride = j * (query.Width + 10.0f);
                var xBegin = isFirstPlayer ? -query.Width : Graphics.LogicalSize.Width + query.Width;
                var xEnd = isFirstPlayer ? 10.0f + stride : Graphics.LogicalSize.Width - query.Width - 10.0f - stride;

                var x = Mathematics.SmoothStep(xBegin, xEnd, 1.0f - _fade);

                var score = Scores[i];

                var hasReached = score >= j + 1;
                Graphics.Draw(texture, null, Matrix3x2.CreateTranslation(x, 10.0f),
                    hasReached ? Color.White : Color.Black);
            }
        }

        public static bool GetWinner(out sbyte playerIndex)
        {
            if (_info.ScoreCount <= 0)
            {
                playerIndex = -1;
                return false;
            }

            for (sbyte i = 0; i < PlayerCount; i++)
            {
                if (Scores[i] < _info.ScoreCount) continue;

                playerIndex = i;
                return true;
            }

            playerIndex = -1;
            return false;
        }
    }
}
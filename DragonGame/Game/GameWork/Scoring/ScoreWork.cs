#region

using System.Drawing;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;

#endregion

namespace DuckDuckJump.Game.GameWork.Scoring;

internal static class ScoreWork
{
    private static readonly sbyte[] Scores = new sbyte[Match.PlayerCount];

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
        if (Match.Info.ScoreCount <= 0) return;

        for (var i = 0; i < Match.PlayerCount; i++)
        for (var j = 0; j < Match.Info.ScoreCount; j++)
        {
            var isFirstPlayer = (i + 1) % 2 != 0;

            var texture = MatchAssets.Texture(MatchAssets.TextureIndex.ScoreIcon);

            var query = texture.QueryTexture();

            var stride = j * (query.Width + 10.0f);
            var xBegin = isFirstPlayer ? -query.Width : Graphics.LogicalSize.Width + query.Width;
            var xEnd = isFirstPlayer ? 10.0f + stride : Graphics.LogicalSize.Width - query.Width - 10.0f - stride;

            var x = Mathematics.SmoothStep(xBegin, xEnd, 1.0f - Match.Fade);

            var score = Scores[i];

            var hasReached = score >= j + 1;
            Graphics.Draw(texture, null, Matrix3x2.CreateTranslation(x, 10.0f),
                hasReached ? Color.White : Color.Black);
        }
    }

    public static bool GetWinner(out MatchWinner winner)
    {
        if (Match.Info.ScoreCount <= 0)
        {
            winner = MatchWinner.None;
            return false;
        }

        for (sbyte i = 0; i < Match.PlayerCount; i++)
        {
            if (Scores[i] < Match.Info.ScoreCount) continue;

            winner = (MatchWinner) i;
            return true;
        }

        winner = MatchWinner.None;
        return false;
    }
}
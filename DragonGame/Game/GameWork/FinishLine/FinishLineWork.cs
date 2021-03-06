#region

using System.Drawing;
using System.IO;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;
using DuckDuckJump.Game.GameWork.Platforming;
using DuckDuckJump.Game.GameWork.Players;

#endregion

namespace DuckDuckJump.Game.GameWork.FinishLine;

internal static class FinishLineWork
{
    private static float _y;
    private static float _alpha;
    private static float _alphaSpeed;

    public static void SaveMe(Stream stream)
    {
        stream.Write(_y);
        stream.Write(_alpha);
        stream.Write(_alphaSpeed);
    }

    public static void LoadMe(Stream stream)
    {
        _y = stream.Read<float>();
        _alpha = stream.Read<float>();
        _alphaSpeed = stream.Read<float>();
    }

    public static void Reset()
    {
        _y = PlatformWork.MaxPlatformY - 250.0f;
    }

    public static void UpdateMe()
    {
        _alpha = Mathematics.SmoothDamp(_alpha, Match.State == Match.MatchState.InGame ? 1.0f : 0.0f, ref _alphaSpeed,
            0.25f,
            GameFlow.TimeStep);
        if (Match.State != Match.MatchState.InGame)
            return;

        for (var i = 0; i < Match.PlayerCount; i++)
        {
            if (!IntersectsPlayer(PlayerWork.Get(i))) continue;

            for (var j = 0; j < Match.PlayerCount; j++)
            {
                if (i == j)
                    continue;
                PlayerWork.Get(j).Lost = true;
            }
        }
    }

    public static void DrawMe()
    {
        Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.FinishLine), null,
            Matrix3x2.CreateTranslation(-Graphics.LogicalSize.Width, _y),
            Color.FromArgb((int)(_alpha * byte.MaxValue), 255, 255, 255));
        Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.FinishLine), null,
            Matrix3x2.CreateTranslation(0.0f, _y),
            Color.FromArgb((int)(_alpha * byte.MaxValue), 255, 255, 255));
        Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.FinishLine), null,
            Matrix3x2.CreateTranslation(Graphics.LogicalSize.Width, _y),
            Color.FromArgb((int)(_alpha * byte.MaxValue), 255, 255, 255));
    }

    private static bool IntersectsPlayer(Player player)
    {
        return player.Position.Y <= _y;
    }
}
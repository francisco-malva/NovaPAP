#region

using System;
using System.Numerics;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game.GameWork.Players;

internal static class PlayerWork
{
    private static readonly Player[] MyPlayers = new Player[Match.PlayerCount];
    public static ref Player First => ref Get(0);
    public static ref Player Last => ref Get(Match.PlayerCount - 1);

    public static ref Player Get(int index)
    {
        return ref MyPlayers[index];
    }

    public static void Reset()
    {
        for (byte i = 0; i < Match.PlayerCount; i++) Get(i).Reset(i);
    }

    public static void UpdateUs(Span<GameInput> inputs)
    {
        for (var i = 0; i < Match.PlayerCount; i++) Get(i).UpdateMe(inputs[i]);

        if (Match.State != Match.MatchState.InGame) return;
        for (var i = 0; i < Match.PlayerCount; i++) Get(i).ApplySpeed();

        if (!Match.Info.ComLevels.HasAi) ApplyPushback();
        ClampPositions();
    }

    private static void ClampPositions()
    {
        for (var i = 0; i < Match.PlayerCount; i++) Get(i).ClampPosition();
    }

    private static void ApplyPushback()
    {
        var intersection = First.PushBox;
        intersection.Intersect(Last.PushBox);

        if (intersection.IsEmpty) return;

        var depth = intersection.Width / 2.0f;

        var direction = First.Position == Last.Position
            ? 1.0f
            : Vector2.Normalize(Last.Position - First.Position).Length();

        var pushback = depth * direction;

        First.Position -= new Vector2(pushback, 0.0f);
        Last.Position += new Vector2(pushback, 0.0f);
    }

    public static void DrawUs()
    {
        for (var i = 0; i < Match.PlayerCount; i++)
            if (Match.State == Match.MatchState.Winner)
            {
                if (Match.RoundWinner == (MatchWinner)i)
                    Get(i).DrawMe();
            }
            else
            {
                Get(i).DrawMe();
            }
    }
}
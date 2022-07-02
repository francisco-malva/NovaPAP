#region

using System;
using System.Numerics;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game.GameWork.Players;

internal static class PlayerWork
{
    private static readonly Player[] MyPlayers = new Player[Match.PlayerCount];

    static PlayerWork()
    {
        for (var i = 0; i < MyPlayers.Length; i++) MyPlayers[i] = new Player();
    }

    public static Player First => Get(0);
    public static Player Last => Get(Match.PlayerCount - 1);

    public static Player Get(int index)
    {
        return MyPlayers[index];
    }

    public static void Reset()
    {
        for (byte i = 0; i < Match.PlayerCount; i++) Get(i).Reset(i);
    }

    public static void UpdateUs(Span<GameInput> inputs)
    {
        var count = (Match.Info.GameFlags & GameInfo.Flags.EndlessClimber) != 0 ? 1 : Match.PlayerCount;

        for (var i = 0; i < count; i++) Get(i).Update(inputs[i]);

        if (Match.State != Match.MatchState.InGame) return;
        for (var i = 0; i < count; i++) Get(i).ApplySpeed();

        if ((Match.Info.GameFlags & GameInfo.Flags.EndlessClimber) == 0) ApplyPushback();
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
        var count = (Match.Info.GameFlags & GameInfo.Flags.EndlessClimber) != 0 ? 1 : Match.PlayerCount;

        for (var i = 0; i < count; i++)
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
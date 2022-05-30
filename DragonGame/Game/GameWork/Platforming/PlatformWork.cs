#region

using System;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.GameWork.Players;
using DuckDuckJump.Game.GameWork.Rng;

#endregion

namespace DuckDuckJump.Game.GameWork.Platforming;

internal static class PlatformWork
{
    private const short MaxPlatformCount = 1024;

    private const float StepRange = 85.0f;

    private static readonly Platform[] Platforms = new Platform[MaxPlatformCount];
    private static readonly float MinimumStep = Platform.Extents.Height * 7.5f;

    public static float MaxPlatformY { get; private set; }

    public static void Reset()
    {
        var lastType = Platform.BehaviorType.Static;

        var spawnY = Graphics.LogicalSize.Height - Platform.Extents.Height - 60.0f;
        for (var i = 0; i < Match.Info.PlatformCount; i++)
        {
            var progression = (float) i / Match.Info.PlatformCount;

            var yStep = RandomWork.Next(MinimumStep, MinimumStep + StepRange * progression);

            spawnY -= yStep;

            Platforms[i].Position =
                new Vector2(
                    Math.Max(0.0f,
                        RandomWork.Next(0.0f, 1.0f) * Graphics.LogicalSize.Width - Platform.Extents.Width),
                    spawnY);

            Platform.BehaviorType newType;
            do
            {
                newType = (Platform.BehaviorType) RandomWork.Next(0, (int) Platform.BehaviorType.Max);
            } while (i > 0 && newType == lastType);

            lastType = newType;
            Platforms[i].Type = newType;

            Platforms[i].ResetMe();

            MaxPlatformY = Platforms[i].Position.Y;
        }
    }

    public static void Update()
    {
        if (Match.State != Match.MatchState.InGame)
            return;
        for (var i = 0; i < Match.Info.PlatformCount; i++) Platforms[i].UpdateMe();
    }

    public static void DrawUs()
    {
        for (var i = 0; i < Match.Info.PlatformCount; i++) Platforms[i].DrawMe();
    }


    public static ref Platform GetPlatform(short index)
    {
        return ref Platforms[index];
    }

    public static bool GetIntersectingPlatform(Player player, out short platformIndex)
    {
        for (short i = 0; i < Match.Info.PlatformCount; i++)
        {
            ref var platform = ref GetPlatform(i);

            if (!platform.OnScreen || !platform.Body.IntersectsWith(player.PlatformBox)) continue;
            platformIndex = i;
            return true;
        }

        platformIndex = -1;
        return false;
    }
}
#region

using System;
using System.IO;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Utilities;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    public static partial class PlatformWork
    {
        private const short MaxPlatformCount = 1024;

        private const float StepRange = 85.0f;

        private static readonly Platform[] Platforms = new Platform[MaxPlatformCount];
        private static readonly float MinimumStep = Platform.Extents.Height * 7.5f;

        public static float MaxPlatformY { get; private set; }


        public static void SaveMe(Stream stream)
        {
            stream.Write(MaxPlatformY);
            for (var i = 0; i < _info.PlatformCount; i++) Platforms[i].Save(stream);
        }

        public static void LoadMe(Stream stream)
        {
            MaxPlatformY = stream.Read<float>();
            for (var i = 0; i < _info.PlatformCount; i++) Platforms[i].Load(stream);
        }

        public static void Reset()
        {
            var lastType = Platform.BehaviorType.Static;

            var spawnY = Graphics.LogicalSize.Height - Platform.Extents.Height - 60.0f;
            for (var i = 0; i < _info.PlatformCount; i++)
            {
                var progression = (float) i / _info.PlatformCount;

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
            if (State != MatchState.InGame)
                return;
            for (var i = 0; i < _info.PlatformCount; i++) Platforms[i].UpdateMe();
        }

        public static void DrawUs()
        {
            for (var i = 0; i < _info.PlatformCount; i++) Platforms[i].DrawMe();
        }


        public static ref Platform GetPlatform(short index)
        {
            return ref Platforms[index];
        }

        public static bool GetIntersectingPlatform(ref PlayerWork.Player player, out short platformIndex)
        {
            for (short i = 0; i < _info.PlatformCount; i++)
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
}
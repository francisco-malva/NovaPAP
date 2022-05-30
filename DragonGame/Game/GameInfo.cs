#region

using System;
using DuckDuckJump.Game.GameWork.Banner;

#endregion

namespace DuckDuckJump.Game;

internal struct GameInfo
{
    [Flags]
    public enum Flags : byte
    {
        None = 0,
        Exhibition = 1,
        NoItems = 2,
        All = byte.MaxValue
    }

    public ComInfo ComInfo;
    public readonly ushort PlatformCount;
    public readonly sbyte ScoreCount;
    public int RandomSeed;
    public readonly ushort TimeLeft;
    public readonly BannerWork.MessageIndex BeginMessageIndex;
    public readonly Flags GameFlags;

    public GameInfo(ComInfo info, ushort platformCount, int randomSeed, sbyte scoreCount,
        ushort timeLeft, BannerWork.MessageIndex beginMessageIndex, Flags flags)
    {
        ComInfo = info;
        PlatformCount = platformCount;
        RandomSeed = randomSeed;
        ScoreCount = scoreCount;
        GameFlags = flags;
        TimeLeft = timeLeft;
        BeginMessageIndex = beginMessageIndex;
    }
}
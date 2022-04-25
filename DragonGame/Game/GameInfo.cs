#region

using System;
using System.IO;

#endregion

namespace DuckDuckJump.Game;

internal unsafe struct GameInfo
{
    public ComLevels ComLevels;
    public readonly int PlatformCount;
    public readonly int RandomSeed;
    public readonly sbyte ScoreCount;
    public readonly bool FightMessages;
    public readonly ulong TimeLeft;

    public GameInfo(ComLevels levels, int platformCount, int randomSeed, sbyte scoreCount, bool fightMessages,
        ulong timeLeft)
    {
        ComLevels = levels;
        PlatformCount = platformCount;
        RandomSeed = randomSeed;
        ScoreCount = scoreCount;
        FightMessages = fightMessages;
        TimeLeft = timeLeft;
    }

    public void Save(Stream stream)
    {
        fixed (GameInfo* ptr = &this)
        {
            var store = new Span<byte>(ptr, sizeof(GameInfo));
            stream.Write(store);
        }
    }

    public void Load(Stream stream)
    {
        fixed (GameInfo* ptr = &this)
        {
            var store = new Span<byte>(ptr, sizeof(GameInfo));
            stream.Read(store);
        }
    }
}
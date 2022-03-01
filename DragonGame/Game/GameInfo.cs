using System.IO;
using DuckDuckJump.Game.Gameplay.Players.AI;

namespace DuckDuckJump.Game;

internal class GameInfo
{
    public readonly AiDifficulty Difficulty;
    public readonly bool HasItems;

    public readonly bool P1Ai;
    public readonly bool P2Ai;
    public readonly ushort PlatformCount;

    public readonly int RandomSeed;
    public readonly byte RoundsToWin;


    public GameInfo(ushort platformCount, byte roundsToWin, bool p1Ai, bool p2Ai, int randomSeed,
        AiDifficulty difficulty, bool hasItems)
    {
        PlatformCount = platformCount;
        RoundsToWin = roundsToWin;
        P1Ai = p1Ai;
        P2Ai = p2Ai;
        RandomSeed = randomSeed;
        Difficulty = difficulty;
        HasItems = hasItems;
    }

    public GameInfo(BinaryReader reader)
    {
        PlatformCount = reader.ReadUInt16();
        RoundsToWin = reader.ReadByte();
        P1Ai = reader.ReadBoolean();
        P2Ai = reader.ReadBoolean();
        RandomSeed = reader.ReadInt32();
        Difficulty = (AiDifficulty) reader.ReadByte();
        HasItems = reader.ReadBoolean();
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(PlatformCount);
        writer.Write(RoundsToWin);
        writer.Write(P1Ai);
        writer.Write(P2Ai);
        writer.Write(RandomSeed);
        writer.Write((byte) Difficulty);
        writer.Write(HasItems);
    }
}
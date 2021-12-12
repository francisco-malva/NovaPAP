using System.IO;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;

namespace DuckDuckJump.Scenes.Game.Gameplay;

internal class GameInfo
{
    public readonly ushort PlatformCount;
    public readonly byte RoundsToWin;

    public readonly bool P1Ai;
    public readonly bool P2Ai;

    public readonly int RandomSeed;

    public readonly AiDifficulty Difficulty;

    public GameInfo(ushort platformCount, byte roundsToWin, bool p1Ai, bool p2Ai, int randomSeed, AiDifficulty difficulty)
    {
        PlatformCount = platformCount;
        RoundsToWin = roundsToWin;
        P1Ai = p1Ai;
        P2Ai = p2Ai;
        RandomSeed = randomSeed;
        Difficulty = difficulty;
    }

    public GameInfo(BinaryReader reader)
    {
        PlatformCount = reader.ReadUInt16();
        RoundsToWin = reader.ReadByte();
        P1Ai = reader.ReadBoolean();
        P2Ai = reader.ReadBoolean();
        RandomSeed = reader.ReadInt32();
        Difficulty = (AiDifficulty)reader.ReadByte();
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(PlatformCount);
        writer.Write(RoundsToWin);
        writer.Write(P1Ai);
        writer.Write(P2Ai);
        writer.Write(RandomSeed);
        writer.Write((byte)Difficulty);
    }
}
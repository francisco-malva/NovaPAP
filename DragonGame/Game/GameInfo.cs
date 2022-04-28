#region

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LiteNetLib.Utils;

#endregion

namespace DuckDuckJump.Game;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct GameInfo : INetSerializable
{
    public ComLevels ComLevels;
    public readonly ushort PlatformCount;
    public readonly sbyte ScoreCount;
    public readonly bool NotExhibition;
    public readonly int RandomSeed;
    public readonly ushort TimeLeft;
    public readonly Match.BannerWork.MessageIndex BeginMessageIndex;
    
    public GameInfo(ComLevels levels, ushort platformCount, int randomSeed, sbyte scoreCount, bool notExhibition,
        ushort timeLeft, Match.BannerWork.MessageIndex beginMessageIndex)
    {
        ComLevels = levels;
        PlatformCount = platformCount;
        RandomSeed = randomSeed;
        ScoreCount = scoreCount;
        NotExhibition = notExhibition;
        TimeLeft = timeLeft;
        BeginMessageIndex = beginMessageIndex;
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

    public void Serialize(NetDataWriter writer)
    {
        fixed (GameInfo* ptr = &this)
        {
            var store = new Span<byte>(ptr, sizeof(GameInfo));
            writer.PutBytesWithLength(store.ToArray());
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        fixed (GameInfo* ptr = &this)
        {
            var dest = reader.GetBytesWithLength();
            fixed(void* dataPtr = dest)
                Unsafe.CopyBlock(ptr, dataPtr, (uint) dest.Length);
        }
    }
}
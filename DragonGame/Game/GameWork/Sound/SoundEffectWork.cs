#region

using System.IO;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Game.Assets;

#endregion

namespace DuckDuckJump.Game.GameWork.Sound;

internal static class SoundEffectWork
{
    private static byte _queuedSfxCount;
    private static readonly Queued[] QueuedEffects = new Queued[16];

    public static void Reset()
    {
        _queuedSfxCount = 0;
    }

    public static void SaveMe(Stream stream)
    {
        stream.Write(_queuedSfxCount);
        for (var i = 0; i < _queuedSfxCount; i++) QueuedEffects[i].SaveSfx(stream);
    }

    public static void LoadMe(Stream stream)
    {
        _queuedSfxCount = stream.Read<byte>();
        for (var i = 0; i < _queuedSfxCount; i++) QueuedEffects[i].LoadSfx(stream);
    }

    public static void Queue(MatchAssets.SfxIndex index, float volume = 1.0f)
    {
        if (_queuedSfxCount == QueuedEffects.Length)
            return;

        ref var queue = ref QueuedEffects[_queuedSfxCount];

        queue.Index = index;
        queue.Volume = volume;

        ++_queuedSfxCount;
    }

    public static void UpdateMe()
    {
        if ((Match.Info.GameFlags & GameInfo.Flags.Exhibition) != 0)
            return;

        for (var i = 0; i < _queuedSfxCount; i++)
            Audio.PlaySound(MatchAssets.SoundEffect(QueuedEffects[i].Index), QueuedEffects[i].Volume);
    }

    private struct Queued
    {
        public MatchAssets.SfxIndex Index;
        public float Volume;

        public void SaveSfx(Stream stream)
        {
            stream.Write(Index);
            stream.Write(Volume);
        }

        public void LoadSfx(Stream stream)
        {
            Index = stream.Read<MatchAssets.SfxIndex>();
            Volume = stream.Read<float>();
        }
    }
}
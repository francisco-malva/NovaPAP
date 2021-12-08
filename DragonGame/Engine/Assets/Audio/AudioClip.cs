using System;
using ManagedBass;

namespace DuckDuckJump.Engine.Assets.Audio
{
    internal class AudioClip : IDisposable
    {
        private readonly bool _isStream;

        public AudioClip(string file, long offset, int length, int playbackCount, BassFlags flags)
        {
            Handle = Bass.SampleLoad(file, offset, length, playbackCount, flags);

            _isStream = false;
        }

        public int Handle { get; }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }


        private void ReleaseUnmanagedResources()
        {
            if (_isStream)
                Bass.StreamFree(Handle);
            else
                Bass.SampleFree(Handle);
        }

        ~AudioClip()
        {
            ReleaseUnmanagedResources();
        }
    }
}
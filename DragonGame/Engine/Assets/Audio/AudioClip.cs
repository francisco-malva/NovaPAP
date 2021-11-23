using System;
using ManagedBass;

namespace DragonGame.Engine.Assets.Audio
{
    internal class AudioClip : IDisposable
    {
        private readonly bool _isStream;
        public int Handle { get; }

        public AudioClip(string file,long offset, int length, int playbackCount, BassFlags flags)
        {
            Handle = Bass.SampleLoad(file, offset, length, playbackCount, flags);

            _isStream = false;
        }


        private void ReleaseUnmanagedResources()
        {
            if (_isStream)
            {
                Bass.StreamFree(Handle);
            }
            else
            {
                Bass.SampleFree(Handle);
            }
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~AudioClip()
        {
            ReleaseUnmanagedResources();
        }
    }
}

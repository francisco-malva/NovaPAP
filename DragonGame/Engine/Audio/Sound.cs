using System;
using ManagedBass;

namespace DragonGame.Engine.Audio
{
    internal class Sample : IDisposable
    {
        public int Handle { get; private set; }
        public Sample(string file, long offset,int length,int playbacks, BassFlags flags)
        {
            Handle = Bass.SampleLoad(file, offset, length, playbacks, flags);
        }

        private void ReleaseUnmanagedResources()
        {
            Bass.SampleFree(Handle);
            Handle = 0;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~Sample()
        {
            ReleaseUnmanagedResources();
        }
    }
}

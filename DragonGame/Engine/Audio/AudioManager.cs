using System;
using ManagedBass;

namespace DragonGame.Engine.Audio
{
    internal class AudioManager : IDisposable
    {
        public AudioManager()
        {
            Bass.Init();
        }

        private static void ReleaseUnmanagedResources()
        {
            Bass.Free();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~AudioManager()
        {
            ReleaseUnmanagedResources();
        }
    }
}

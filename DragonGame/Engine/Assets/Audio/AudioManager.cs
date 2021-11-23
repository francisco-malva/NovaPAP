using ManagedBass;

namespace DragonGame.Engine.Assets.Audio
{
    internal class AudioManager : ResourceManager<AudioClip>
    {

        public AudioManager() : base("Assets/Audio", "ogg")
        {
            Bass.Init();
        }

        protected override void ReleaseUnmanagedResources()
        {
            Bass.Free();
        }

        protected override AudioClip LoadAsset(string path)
        {
            return new AudioClip(path, 0, 0, 16, BassFlags.Default);
        }
    }
}

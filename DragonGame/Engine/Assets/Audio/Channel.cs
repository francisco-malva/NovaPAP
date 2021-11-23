using ManagedBass;

namespace DragonGame.Engine.Assets.Audio
{
    public readonly struct Channel
    {
        public readonly int Handle;

        public Channel(int handle)
        {
            Handle = handle;
        }

        public BassFlags ChannelFlags(BassFlags flags, BassFlags mask)
        {
            return Bass.ChannelFlags(Handle, flags, mask);
        }
    }
}
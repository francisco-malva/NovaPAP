using DragonGame.Engine.Assets.Audio;
using ManagedBass;

namespace DuckDuckJump.Scenes.Game.Gameplay.Announcer
{
    internal class Announcer
    {
        private AudioClip[] _clips = new AudioClip[]{
            DragonGame.Engine.Game.Instance.AudioManager["get-ready"],
            DragonGame.Engine.Game.Instance.AudioManager["go"],
            DragonGame.Engine.Game.Instance.AudioManager["Game/Banners/winner"],
            DragonGame.Engine.Game.Instance.AudioManager["Game/Banners/you-lose"],
            DragonGame.Engine.Game.Instance.AudioManager["Game/Banners/draw"]
        };

        public Announcer()
        {
        }

        public void Say(AnnouncementType announcement)
        {
            var channel = Bass.SampleGetChannel(_clips[(int)announcement].Handle);
            Bass.ChannelPlay(channel);
        }
    }
}
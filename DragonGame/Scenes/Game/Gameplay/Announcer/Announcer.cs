using DuckDuckJump.Engine.Assets.Audio;
using ManagedBass;

namespace DuckDuckJump.Scenes.Game.Gameplay.Announcer
{
    internal class Announcer
    {
        private readonly AudioClip[] _clips =
        {
            Engine.Game.Instance.AudioManager["get-ready"],
            Engine.Game.Instance.AudioManager["go"],
            Engine.Game.Instance.AudioManager["Game/Banners/winner"],
            Engine.Game.Instance.AudioManager["Game/Banners/you-lose"],
            Engine.Game.Instance.AudioManager["Game/Banners/draw"]
        };

        public void Say(AnnouncementType announcement)
        {
            var channel = Bass.SampleGetChannel(_clips[(int) announcement].Handle);
            Bass.ChannelPlay(channel);
        }
    }
}
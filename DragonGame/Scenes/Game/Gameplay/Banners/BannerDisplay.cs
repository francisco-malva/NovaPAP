using DuckDuckJump.Engine.Wrappers.SDL2;

namespace DuckDuckJump.Scenes.Game.Gameplay.Banners
{
    internal class BannerDisplay
    {
        private ushort _bannerDuration;

        private readonly Texture[] _banners =
        {
            Engine.Game.Instance.TextureManager["Game/Banners/get-ready"],
            Engine.Game.Instance.TextureManager["Game/Banners/go"],
            Engine.Game.Instance.TextureManager["Game/Banners/winner"],
            Engine.Game.Instance.TextureManager["Game/Banners/you-lose"],
            Engine.Game.Instance.TextureManager["Game/Banners/draw"]
        };

        private ushort _bannerTimer;

        private BannerType _currentBanner;

        private Texture BannerTexture => _banners[(int)_currentBanner];


        public void Raise(BannerType type, ushort duration)
        {
            _currentBanner = type;
            _bannerTimer = duration;
            _bannerDuration = duration;
        }


        public void Update()
        {
            if (_bannerTimer > 0) --_bannerTimer;
        }

        public void Draw()
        {
            if (_bannerTimer != 0)
            {
                BannerTexture.SetAlphaMod((byte)(_bannerTimer / (float)_bannerDuration * 255.0f));
                Engine.Game.Instance.Renderer.Copy(BannerTexture, null,
                    new Rectangle(GameField.Width / 2 - BannerTexture.Width / 2,
                        GameField.Height / 2 - BannerTexture.Height / 2, BannerTexture.Width, BannerTexture.Height));
                --_bannerTimer;
            }
        }
    }
}
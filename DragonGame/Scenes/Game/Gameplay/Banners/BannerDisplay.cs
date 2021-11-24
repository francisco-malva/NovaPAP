using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay;

namespace DuckDuckJump.Scenes.Game.Gameplay
{
    internal class BannerDisplay
    {
        private Texture[] _banners = new Texture[]
        {
            DragonGame.Engine.Game.Instance.TextureManager["Game/Banners/get-ready"],
            DragonGame.Engine.Game.Instance.TextureManager["Game/Banners/go"],
            DragonGame.Engine.Game.Instance.TextureManager["Game/Banners/winner"],
            DragonGame.Engine.Game.Instance.TextureManager["Game/Banners/you-lose"],
            DragonGame.Engine.Game.Instance.TextureManager["Game/Banners/draw"],
        };

        private ushort _bannerDuration;
        private ushort _bannerTimer;

        private BannerType _currentBanner;


        public void Raise(BannerType type, ushort duration)
        {
            _currentBanner = type;
            _bannerTimer = duration;
            _bannerDuration = duration;
        }


        public void Update()
        {
            if (_bannerTimer > 0)
            {
                --_bannerTimer;
            }
        }

        public void Draw()
        {
            if (_bannerTimer != 0)
            {
                BannerTexture.SetAlphaMod((byte)(_bannerTimer / (float)_bannerDuration * 255.0f));
                DragonGame.Engine.Game.Instance.Renderer.Copy(BannerTexture, null,
                    new Rectangle(GameField.Width / 2 - BannerTexture.Width / 2,
                        GameField.Height / 2 - BannerTexture.Height / 2, BannerTexture.Width, BannerTexture.Height));
                --_bannerTimer;
            }
        }

        private Texture BannerTexture => _banners[(int)_currentBanner];
    }
}